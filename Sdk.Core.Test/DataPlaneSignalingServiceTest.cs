using Moq;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Test;

public class DataPlaneSignalingServiceTest
{
    private readonly Mock<IDataPlaneStore> _dataPlaneStore;
    private readonly DataPlaneSdk _sdk;
    private readonly DataPlaneSignalingService service;

    public DataPlaneSignalingServiceTest()
    {
        _dataPlaneStore = new Mock<IDataPlaneStore>();
        _sdk = new DataPlaneSdk();
        service = new DataPlaneSignalingService(_dataPlaneStore.Object, _sdk);
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = TestMethods.CreateDataFlow(dataFlowId);

        _dataPlaneStore.Setup(store => store.FindByIdAsync(dataFlowId))
            .ReturnsAsync(dataFlow);

        var result = await service.TerminateAsync(dataFlowId, reason);

        Assert.True(result.IsSucceeded);
        _dataPlaneStore.Verify(store => store.FindByIdAsync(dataFlowId), Times.Once);
        _dataPlaneStore.Verify(store => store.SaveAsync(dataFlow), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_VerifySdkEventInvoked()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = TestMethods.CreateDataFlow(dataFlowId);

        _dataPlaneStore.Setup(store => store.FindByIdAsync(dataFlowId))
            .ReturnsAsync(dataFlow);

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Success(default));

        _sdk.OnTerminate += eventMock.Object;

        var result = await service.TerminateAsync(dataFlowId, reason);

        Assert.True(result.IsSucceeded);
        _dataPlaneStore.Verify(store => store.FindByIdAsync(dataFlowId), Times.Once);
        _dataPlaneStore.Verify(store => store.SaveAsync(dataFlow), Times.Once);
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_DataFlowNotFound()
    {
        const string dataFlowId = "test-flow-id";

        _dataPlaneStore.Setup(store => store.FindByIdAsync(dataFlowId))
            .ReturnsAsync(null as DataFlow);
        var result = await service.TerminateAsync(dataFlowId);
        Assert.False(result.IsSucceeded);
        Assert.Equal(404, result.Failure!.Code);
    }
}