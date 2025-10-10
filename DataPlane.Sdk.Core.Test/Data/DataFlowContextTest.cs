using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using DataPlane.Sdk.Test.Utils;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace DataPlane.Sdk.Core.Test.Data;

public abstract class DataFlowContextTest(DataFlowContext context, string testLockId)
{
    [Fact]
    public async Task SaveAsync_ShouldAddNewDataFlow()
    {
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.UpsertAsync(dataFlow);

        context.ChangeTracker.HasChanges().ShouldBeTrue();
        var entry = context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        entry.ShouldNotBeNull();
        entry.State.ShouldBe(EntityState.Added);

        entry.Entity.Id.ShouldBe(dataFlow.Id);
        entry.Entity.ShouldBeEquivalentTo(dataFlow);
        entry.Entity.Destination.Properties["test-key"].ShouldBeEquivalentTo("test-value");
    }

    [Fact]
    public async Task SaveAsync_ShouldUpdateExistingDataFlow()
    {
        //create data flow
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.DataFlows.AddAsync(dataFlow);
        await context.SaveChangesAsync();

        // update, call save
        dataFlow.State = DataFlowState.Completed;
        await context.UpsertAsync(dataFlow);

        context.ChangeTracker.HasChanges().ShouldBeTrue();
        var entry = context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        entry.ShouldNotBeNull();
        entry.State.ShouldBe(EntityState.Modified);
        entry.Entity.State.ShouldBe(DataFlowState.Completed);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnDataFlow_WhenExists()
    {
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.DataFlows.AddAsync(dataFlow);
        await context.SaveChangesAsync();

        var foundFlow = await context.FindByIdAsync(dataFlow.Id);

        foundFlow.ShouldNotBeNull();
        foundFlow.Id.ShouldBe(dataFlow.Id);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExist()
    {
        var foundFlow = await context.FindByIdAsync("non-existent-id");
        foundFlow.ShouldBeNull();
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnDataFlow_WhenExistsAndLeaseAcquired()
    {
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.DataFlows.AddAsync(dataFlow);
        await context.SaveChangesAsync();

        var result = await context.FindByIdAndLeaseAsync(dataFlow.Id);
        // verify data flow
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.Id.ShouldBe(dataFlow.Id);
        context.ChangeTracker.HasChanges().ShouldBeFalse(); // FindByIdAndLease should commit transaction

        //verify lease
        var lease = await context.Leases.FindAsync(dataFlow.Id);
        lease.ShouldNotBeNull();
        lease.LeasedBy.ShouldBe(testLockId);
        lease.IsExpired().ShouldBeFalse("lease should not be expired");
        DateTimeOffset.FromUnixTimeMilliseconds(lease.LeasedAt).DateTime.ShouldBeLessThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await context.FindByIdAndLeaseAsync("not-exist");
        result.IsFailed.ShouldBeTrue();
        result.Failure!.Reason.ShouldBe(FailureReason.NotFound);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldFail_WhenAlreadyLeased()
    {
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.DataFlows.AddAsync(dataFlow);
        await context.Leases.AddAsync(new Lease
            {
                EntityId = dataFlow.Id,
                LeasedBy = "someone_else",
                LeasedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LeaseDurationMillis = 60_000 // 1 minute
            }
        );
        await context.SaveChangesAsync();

        var result = await context.FindByIdAndLeaseAsync(dataFlow.Id);
        result.IsFailed.ShouldBeTrue();
        result.Failure!.Reason.ShouldBe(FailureReason.Conflict);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldSucceed_WhenAlreadyLeasedBySelf()
    {
        var dataFlow = TestMethods.CreateDataFlow("id-" + Guid.NewGuid());
        await context.DataFlows.AddAsync(dataFlow);
        var originalLease = new Lease
        {
            EntityId = dataFlow.Id,
            LeasedBy = testLockId,
            LeasedAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(20)).ToUnixTimeMilliseconds(),
            LeaseDurationMillis = 60_000 // 1 minute
        };
        await context.Leases.AddAsync(originalLease);
        await context.SaveChangesAsync();

        var result = await context.FindByIdAndLeaseAsync(dataFlow.Id);
        context.ChangeTracker.HasChanges().ShouldBeFalse(); // FindByIdAndLease should commit transaction
        context.ChangeTracker.Entries<Lease>()
            .FirstOrDefault(l => l.Entity.EntityId == dataFlow.Id)
            .ShouldNotBeSameAs(originalLease);
        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task NextNotLeased()
    {
        var f1 = TestMethods.CreateDataFlow("test-flow-id1", DataFlowState.Started);
        var f2 = TestMethods.CreateDataFlow("test-flow-id2", DataFlowState.Starting);
        var f3 = TestMethods.CreateDataFlow("test-flow-id3", DataFlowState.Completed);
        var f4 = TestMethods.CreateDataFlow("test-flow-id4", DataFlowState.Terminated);
        var f5 = TestMethods.CreateDataFlow("test-flow-id5", DataFlowState.Started);
        var f6 = TestMethods.CreateDataFlow("test-flow-id6", DataFlowState.Uninitialized);
        context.DataFlows.AddRange(f1, f2, f3, f4, f5, f6);
        await context.SaveChangesAsync();

        var notLeased = await context.NextNotLeased(100, DataFlowState.Started);
        notLeased.ShouldNotBeNull();
        notLeased.Count.ShouldBe(2);
        notLeased.ShouldContain(f1);
        notLeased.ShouldContain(f5);

        var notified = await context.NextNotLeased(1, DataFlowState.Completed);
        notified.ShouldNotBeNull();
        notified.Count.ShouldBe(1);
        notified.ShouldContain(f3);
    }
}

public class InMem(InMemoryFixture fixture)
    : DataFlowContextTest(fixture.Context, fixture.LockId), IClassFixture<InMemoryFixture>;

public class Postgres(PostgresFixture fixture)
    : DataFlowContextTest(fixture.Context, fixture.LockId), IClassFixture<PostgresFixture>;