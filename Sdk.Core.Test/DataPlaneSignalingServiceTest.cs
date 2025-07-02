using Moq;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;
using Shouldly;
using static Sdk.Core.Data.DataFlowContextFactory;
using static Sdk.Core.Domain.DataFlowState;
using static Sdk.Core.Domain.FailureReason;
using static Sdk.Core.Test.TestMethods;
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
        var runtimeId = "test-lock-id";
        _dataFlowContext = CreateInMem(runtimeId);

        _service = new DataPlaneSignalingService(_dataFlowContext, _sdk, runtimeId);
    }

    public void Dispose()
    {
        _dataFlowContext.DataFlows.RemoveRange(_dataFlowContext.DataFlows);
        _dataFlowContext.SaveChanges();
    }


    [Fact]
    public async Task GetState_WhenExists()
    {
        var flow = CreateDataFlow("test-process-id", Provisioning);
        _dataFlowContext.DataFlows.Add(flow);
        await _dataFlowContext.SaveChangesAsync();
        var result = await _service.GetTransferStateAsync(flow.Id);
        result.ShouldNotBeNull();
        result.Content.ShouldBe(Provisioning);
    }

    [Fact]
    public async Task GetState_WhenNotExists()
    {
        var result = await _service.GetTransferStateAsync("non-existing-id");
        result.ShouldNotBeNull();
        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(NotFound);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnSuccess_WhenDataFlowIsCreated()
    {
        var message = CreateStartMessage();

        var result = await _service.StartAsync(message);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.ShouldSatisfyAllConditions(() => result.Content!.DataAddress.ShouldNotBeNull());


        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string id = "test-process-id";
        var message = CreateStartMessage();
        message.ProcessId = id;

        var dataFlow = CreateDataFlow(id);
        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var result = await _service.StartAsync(message);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.ShouldSatisfyAllConditions(() => result.Content!.DataAddress.ShouldNotBeNull());

        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId && x.State == Started);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnSuccess_WhenDataFlowIsAlreadyStarted()
    {
        var startMessage = CreateStartMessage();
        var dataFlow = CreateDataFlow(startMessage.ProcessId, Started);
        await _dataFlowContext.AddAsync(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var mock = new Mock<Func<DataFlow, StatusResult<DataFlowResponseMessage>>>();
        _sdk.OnStart += mock.Object;

        var result = await _service.StartAsync(startMessage);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        mock.Verify(m => m.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnFailure_WhenSdkReportsFailure()
    {
        var startMessage = CreateStartMessage();

        var mock = new Mock<Func<DataFlow, StatusResult<DataFlowResponseMessage>>>();

        mock.Setup(m => m.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<DataFlowResponseMessage>.Conflict("error"));
        _sdk.OnStart += mock.Object;

        var result = await _service.StartAsync(startMessage);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("error");

        (await _dataFlowContext.DataFlows.FindAsync(startMessage.ProcessId)).ShouldBeNull();

        mock.Verify(m => m.Invoke(It.IsAny<DataFlow>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnFailure_WhenDataFlowIsLeased()
    {
        var startMessage = CreateStartMessage();
        var dataFlow = CreateDataFlow(startMessage.ProcessId, Started);
        var lease = new Lease
        {
            LeasedBy = "someone-else",
            LeaseDurationMillis = 60_000,
            EntityId = dataFlow.Id
        };
        await _dataFlowContext.DataFlows.AddAsync(dataFlow);
        await _dataFlowContext.Leases.AddAsync(lease);
        await _dataFlowContext.SaveChangesAsync();

        var mock = new Mock<Func<DataFlow, StatusResult<DataFlowResponseMessage>>>();
        _sdk.OnStart += mock.Object;

        var result = await _service.StartAsync(startMessage);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(Conflict);
        mock.Verify(m => m.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = CreateDataFlow(dataFlowId);

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
        var dataFlow = CreateDataFlow(dataFlowId);

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
        result.Failure!.Reason.ShouldBe(NotFound);
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnFailure_WhenSdkReportsFailure()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Conflict("foobartestmessage"));

        _sdk.OnTerminate += eventMock.Object;

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("foobartestmessage");

        (await _dataFlowContext.DataFlows.FindAsync(dataFlow.Id))!.State.ShouldNotBe(Terminated);

        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnSuccess_WhenAlreadyTerminated()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Terminated;

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task SuspendAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == dataFlowId);
    }

    [Fact]
    public async Task SuspendAsync_VerifySdkEventInvoked()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Success(default));

        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task SuspendAsync_DataFlowNotFound()
    {
        const string dataFlowId = "test-flow-id";

        var result = await _service.SuspendAsync(dataFlowId);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Reason.ShouldBe(NotFound);
    }

    [Fact]
    public async Task SuspendAsync_ShouldReturnFailure_WhenSdkReportsFailure()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Conflict("foobartestmessage"));

        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("foobartestmessage");

        (await _dataFlowContext.DataFlows.FindAsync(dataFlow.Id))!.State.ShouldNotBe(Suspended);

        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task SuspendAsync_ShouldReturnSuccess_WhenAlreadySuspended()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Suspended;

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Never);
    }
}