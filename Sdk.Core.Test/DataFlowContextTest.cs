using Microsoft.EntityFrameworkCore;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Shouldly;

namespace Sdk.Core.Test;

public class DataFlowContextTest
{
    private readonly DataFlowContext _context = DataFlowContextFactory.CreateInMem("test-lock-id");

    [Fact]
    public async Task SaveAsync_ShouldAddNewDataFlow()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.SaveAsync(dataFlow);

        _context.ChangeTracker.HasChanges().ShouldBeTrue();
        var entry = _context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        entry.ShouldNotBeNull();
        entry.State.ShouldBe(EntityState.Added);

        entry.Entity.Id.ShouldBe(dataFlow.Id);
    }

    [Fact]
    public async Task SaveAsync_ShouldUpdateExistingDataFlow()
    {
        //create data flow
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.DataFlows.AddAsync(dataFlow);
        await _context.SaveChangesAsync();

        // update, call save
        dataFlow.State = (int)DataFlowState.Completed;
        await _context.SaveAsync(dataFlow);

        _context.ChangeTracker.HasChanges().ShouldBeTrue();
        var entry = _context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        entry.ShouldNotBeNull();
        entry.State.ShouldBe(EntityState.Modified);
        entry.Entity.State.ShouldBe((int)DataFlowState.Completed);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnDataFlow_WhenExists()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.DataFlows.AddAsync(dataFlow);
        await _context.SaveChangesAsync();

        var foundFlow = await _context.FindByIdAsync(dataFlow.Id);

        foundFlow.ShouldNotBeNull();
        foundFlow.Id.ShouldBe(dataFlow.Id);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExist()
    {
        var foundFlow = await _context.FindByIdAsync("non-existent-id");
        foundFlow.ShouldBeNull();
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnDataFlow_WhenExistsAndLeaseAcquired()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.DataFlows.AddAsync(dataFlow);
        await _context.SaveChangesAsync();

        var result = await _context.FindByIdAndLeaseAsync(dataFlow.Id);
        // verify data flow
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.Id.ShouldBe(dataFlow.Id);

        //verify lease
        var lease = await _context.Leases.FindAsync(dataFlow.Id);
        lease.ShouldNotBeNull();
        lease.LeasedBy.ShouldBe("test-lock-id");
        lease.IsExpired().ShouldBeFalse("lease should not be expired");
        DateTimeOffset.FromUnixTimeMilliseconds(lease.LeasedAt).DateTime.ShouldBeLessThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _context.FindByIdAndLeaseAsync("not-exist");
        result.IsFailed.ShouldBeTrue();
        result.Failure!.Code.ShouldBe(404);
    }
}