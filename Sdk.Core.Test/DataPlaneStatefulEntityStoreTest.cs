using Sdk.Core.Infrastructure;

namespace Sdk.Core.Test;

public class DataPlaneStatefulEntityStoreTest
{
    private readonly DataPlaneStatefulEntityStore _dataPlaneStatefulEntityStore = new("test-lease-id");

    [Fact]
    public async Task FindById_WhenExists()
    {
        var id = "test-id";
        var flow = TestMethods.CreateDataFlow(id);
        await _dataPlaneStatefulEntityStore.SaveAsync(flow);
        var found = await _dataPlaneStatefulEntityStore.FindByIdAsync(id);
        Assert.NotNull(found);
        Assert.Equal(flow, found);
    }

    [Fact]
    public async Task FindById_WhenNotExists()
    {
        Assert.Null(await _dataPlaneStatefulEntityStore.FindByIdAsync("test-id"));
    }

    [Fact]
    public async Task SaveAsync_ShouldOverwriteExisting()
    {
        var id = "duplicate-id";
        var original = TestMethods.CreateDataFlow(id);
        await _dataPlaneStatefulEntityStore.SaveAsync(original);

        var updated = TestMethods.CreateDataFlow(id);
        updated.RuntimeId = "new-runtime";
        await _dataPlaneStatefulEntityStore.SaveAsync(updated);

        var found = await _dataPlaneStatefulEntityStore.FindByIdAsync(id);
        Assert.NotNull(found);
        Assert.Equal("new-runtime", found!.RuntimeId);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateNew()
    {
        var id = "test-id";
        var original = TestMethods.CreateDataFlow(id);
        await _dataPlaneStatefulEntityStore.SaveAsync(original);

        var found = await _dataPlaneStatefulEntityStore.FindByIdAsync(id);
        Assert.Equal(found, original);
    }

    [Fact]
    public async Task FindByIdAndLease()
    {
        const string id = "test-id";
        var dataFlow = TestMethods.CreateDataFlow(id);
        await _dataPlaneStatefulEntityStore.SaveAsync(dataFlow);
        var result = await _dataPlaneStatefulEntityStore.FindByIdAndLeaseAsync(id);
        Assert.True(result.IsSucceeded);
        Assert.Equal(dataFlow, result.Content);
    }

    [Fact]
    public async Task FindByIdAndLease_NotFound()
    {
        const string id = "test-id";
        var result = await _dataPlaneStatefulEntityStore.FindByIdAndLeaseAsync(id);
        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Failure!.Code);
    }

    [Fact]
    public async Task FindByIdAndLease_AlreadyLeasedBySame()
    {
        const string id = "test-id";
        var dataFlow = TestMethods.CreateDataFlow(id);
        await _dataPlaneStatefulEntityStore.SaveAsync(dataFlow);
        var result = await _dataPlaneStatefulEntityStore.FindByIdAndLeaseAsync(id);
        Assert.True(result.IsSucceeded);

        var secondResult = await _dataPlaneStatefulEntityStore.FindByIdAndLeaseAsync(id);
        Assert.True(secondResult.IsSucceeded);
    }

    [Fact]
    public async Task NextNotLeased_ThrowsNotImplemented()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => _dataPlaneStatefulEntityStore.NextNotLeased(0));
    }
}