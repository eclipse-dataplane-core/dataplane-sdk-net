using Moq;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;
using Shouldly;
using Testcontainers.PostgreSql;
using static Sdk.Core.Data.DataFlowContextFactory;
using static Sdk.Core.Domain.DataFlowState;
using static Sdk.Core.Domain.FailureReason;
using static Sdk.Core.Test.TestMethods;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Test;

public abstract class DataPlaneSignalingServiceTest : IDisposable
{
    private readonly DataPlaneSdk _sdk;
    private readonly DataPlaneSignalingService _service;
    protected readonly DataFlowContext DataFlowContext;

    protected DataPlaneSignalingServiceTest(DataFlowContext context)
    {
        var runtimeId = "test-lock-id";
        DataFlowContext = context;

        _sdk = new DataPlaneSdk
        {
            Store = DataFlowContext
        };
        _service = new DataPlaneSignalingService(DataFlowContext, _sdk, runtimeId);
    }


    public void Dispose()
    {
        DataFlowContext.Database.EnsureDeleted();
        DataFlowContext.SaveChanges();
    }


    [Fact]
    public async Task GetState_WhenExists()
    {
        var flow = CreateDataFlow("test-process-id", Provisioning);
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();
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


        DataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        DataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string id = "test-process-id";
        var message = CreateStartMessage();
        message.ProcessId = id;

        var dataFlow = CreateDataFlow(id);
        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var result = await _service.StartAsync(message);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.ShouldSatisfyAllConditions(() => result.Content!.DataAddress.ShouldNotBeNull());

        DataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        DataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId && x.State == Started);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnSuccess_WhenDataFlowIsAlreadyStarted()
    {
        var startMessage = CreateStartMessage();
        var dataFlow = CreateDataFlow(startMessage.ProcessId, Started);
        await DataFlowContext.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();

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

        (await DataFlowContext.DataFlows.FindAsync(startMessage.ProcessId)).ShouldBeNull();

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
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.Leases.AddAsync(lease);
        await DataFlowContext.SaveChangesAsync();

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

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        DataFlowContext.DataFlows.ShouldContain(x => x.Id == dataFlowId);
    }

    [Fact]
    public async Task TerminateAsync_VerifySdkEventInvoked()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test termination";
        var dataFlow = CreateDataFlow(dataFlowId);

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

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

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Conflict("foobartestmessage"));

        _sdk.OnTerminate += eventMock.Object;

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("foobartestmessage");

        (await DataFlowContext.DataFlows.FindAsync(dataFlow.Id))!.State.ShouldNotBe(Terminated);

        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_ShouldReturnSuccess_WhenAlreadyTerminated()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Terminated;

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

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

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        DataFlowContext.DataFlows.ShouldContain(x => x.Id == dataFlowId);
    }

    [Fact]
    public async Task SuspendAsync_VerifySdkEventInvoked()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

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

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<Void>.Conflict("foobartestmessage"));

        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("foobartestmessage");

        (await DataFlowContext.DataFlows.FindAsync(dataFlow.Id))!.State.ShouldNotBe(Suspended);

        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Once);
    }

    [Fact]
    public async Task SuspendAsync_ShouldReturnSuccess_WhenAlreadySuspended()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Suspended;

        DataFlowContext.DataFlows.Add(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult<Void>>>();
        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Never);
    }
}

public class InMemDataPlaneSignalingServiceTest() : DataPlaneSignalingServiceTest(CreateInMem("test-lock-id"));

public class PostgresDataPlaneSignalingServiceTest : DataPlaneSignalingServiceTest, IAsyncDisposable
{
    private static PostgreSqlContainer? _postgreSqlContainer;

    public PostgresDataPlaneSignalingServiceTest() : base(CreatePostgres("Host=localhost;Port=5432;Database=SdkApi;Username=postgres;Password=postgres",
        "test-lock-id"))
    {
        if (_postgreSqlContainer == null)
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithDatabase("SdkApi")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithPortBinding(5432, 5432)
                .Build();
            _postgreSqlContainer.StartAsync().Wait();
        }

        DataFlowContext.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }
}