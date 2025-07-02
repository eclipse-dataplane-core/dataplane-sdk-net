using Moq;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Infrastructure;
using Shouldly;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Test;

public class DataPlaneSignalingServiceTest : IDisposable
{
    private readonly DataFlowContext _dataFlowContext;
    private readonly DataPlaneSdk _sdk;
    private readonly DataPlaneSignalingService _service;

    public DataPlaneSignalingServiceTest()
    {
        _sdk = new DataPlaneSdk();
        _dataFlowContext = DataFlowContextFactory.CreateInMem("test-lock-id");

        _service = new DataPlaneSignalingService(_dataFlowContext, _sdk);
    }

    public void Dispose()
    {
        _dataFlowContext.DataFlows.RemoveRange(_dataFlowContext.DataFlows);
        _dataFlowContext.SaveChanges();
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = TestMethods.CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == dataFlowId);
    }

    [Fact]
    public async Task TerminateAsync_VerifySdkEventInvoked()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = TestMethods.CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Success(default));

        _sdk.OnTerminate += eventMock.Object;

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_DataFlowNotFound()
    {
        const string dataFlowId = "test-flow-id";

        var result = await _service.TerminateAsync(dataFlowId);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Code.ShouldBe(404);
    }
}