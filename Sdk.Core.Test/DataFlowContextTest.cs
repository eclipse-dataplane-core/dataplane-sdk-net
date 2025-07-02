using Microsoft.EntityFrameworkCore;
using Sdk.Core.Data;
using Sdk.Core.Domain;

namespace Sdk.Core.Test;

public class DataFlowContextTest
{
    private readonly DataFlowContext _context = DataFlowContextFactory.CreateInMem("test-lock-id");

    [Fact]
    public async Task SaveAsync_ShouldAddNewDataFlow()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.SaveAsync(dataFlow);

        Assert.True(_context.ChangeTracker.HasChanges());
        var entry = _context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        Assert.NotNull(entry);
        Assert.Equal(EntityState.Added, entry.State);

        Assert.Equal(dataFlow.Id, entry.Entity.Id);
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

        Assert.True(_context.ChangeTracker.HasChanges());
        var entry = _context.ChangeTracker.Entries<DataFlow>().FirstOrDefault(e => e.Entity.Id == dataFlow.Id);
        Assert.NotNull(entry);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.Equal((int)DataFlowState.Completed, entry.Entity.State);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnDataFlow_WhenExists()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.DataFlows.AddAsync(dataFlow);
        await _context.SaveChangesAsync();

        var foundFlow = await _context.FindByIdAsync(dataFlow.Id);

        Assert.NotNull(foundFlow);
        Assert.Equal(dataFlow.Id, foundFlow.Id);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExist()
    {
        var foundFlow = await _context.FindByIdAsync("non-existent-id");
        Assert.Null(foundFlow);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnDataFlow_WhenExistsAndLeaseAcquired()
    {
        var dataFlow = TestMethods.CreateDataFlow("test-flow-id");
        await _context.DataFlows.AddAsync(dataFlow);
        await _context.SaveChangesAsync();

        var result = await _context.FindByIdAndLeaseAsync(dataFlow.Id);
        // verify data flow
        Assert.True(result.IsSucceeded);
        Assert.NotNull(result.Content);
        Assert.Equal(dataFlow.Id, result.Content.Id);

        //verify lease
        var lease = await _context.Leases.FindAsync(dataFlow.Id);
        Assert.NotNull(lease);
        Assert.Equal("test-lock-id", lease.LeasedBy);
        Assert.False(lease.IsExpired(), "lease should not be expired");
        Assert.True(DateTimeOffset.FromUnixTimeMilliseconds(lease.LeasedAt).DateTime < DateTime.UtcNow);
    }

    [Fact]
    public async Task FindByIdAndLeaseAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _context.FindByIdAndLeaseAsync("not-exist");
        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Failure!.Code);
    }
}