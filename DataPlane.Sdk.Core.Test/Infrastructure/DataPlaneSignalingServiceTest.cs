using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using Moq;
using Shouldly;
using Testcontainers.PostgreSql;
using static DataPlane.Sdk.Core.Data.DataFlowContextFactory;
using static DataPlane.Sdk.Core.Domain.Model.DataFlowState;
using static DataPlane.Sdk.Core.Domain.Model.FailureReason;
using static DataPlane.Sdk.Core.Test.TestMethods;

[assembly: CollectionBehavior(MaxParallelThreads = 1)]

namespace DataPlane.Sdk.Core.Test.Infrastructure;

public abstract class DataPlaneSignalingServiceTest : IDisposable
{
    private DataFlowContext _dataFlowContext = null!;
    private DataPlaneSdk _sdk = null!;
    private DataPlaneSignalingService _service = null!;

    public void Dispose()
    {
        _dataFlowContext.Database.EnsureDeleted();
        _dataFlowContext.SaveChanges();
    }

    protected void Initialize(DataFlowContext dataFlowContext)
    {
        var runtimeId = "test-lock-id";

        _dataFlowContext = dataFlowContext;
        _sdk = new DataPlaneSdk
        {
            RuntimeId = runtimeId,
            DataFlowStore = () => _dataFlowContext
        };
        _service = new DataPlaneSignalingService(_dataFlowContext, _sdk);
    }

    [Fact]
    public async Task GetState_WhenExists()
    {
        var flow = CreateDataFlow("test-process-id", Prepared);
        _dataFlowContext.DataFlows.Add(flow);
        await _dataFlowContext.SaveChangesAsync();
        var result = await _service.GetTransferStateAsync(flow.Id);
        result.ShouldNotBeNull();
        result.Content.ShouldBe(Prepared);
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
    public async Task StartAsync_WhenDataFlowIsCreated_ShouldReturnSuccess()
    {
        var message = CreateStartMessage();

        var result = await _service.StartAsync(message);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        result.Content.ShouldSatisfyAllConditions(() => result.Content!.Source.ShouldNotBeNull());
        result.Content.ShouldSatisfyAllConditions(() => result.Content!.Destination.ShouldNotBeNull());


        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId);
    }

    [Fact]
    public async Task StartAsync_WhenDataFlowExists_ShouldReturnConflict()
    {
        const string id = "test-process-id";
        var message = CreateStartMessage();
        message.ProcessId = id;

        var dataFlow = CreateDataFlow(id);
        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var result = await _service.StartAsync(message);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(Conflict);

        // result.Content.ShouldSatisfyAllConditions(() => result.Content!.Source.ShouldNotBeNull());
        // result.Content.ShouldSatisfyAllConditions(() => result.Content!.Destination.ShouldNotBeNull());

        // _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
        // _dataFlowContext.DataFlows.ShouldContain(x => x.Id == message.ProcessId && x.State == Started);
    }

    [Fact]
    public async Task StartAsync_WhenSdkReportsFailure_ShouldReturnFailure()
    {
        var startMessage = CreateStartMessage();

        var mock = new Mock<Func<DataFlow, StatusResult<DataFlow>>>();

        mock.Setup(m => m.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<DataFlow>.Conflict("error"));
        _sdk.OnStart += mock.Object;

        var result = await _service.StartAsync(startMessage);

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Message.ShouldBe("error");

        (await _dataFlowContext.DataFlows.FindAsync(startMessage.ProcessId)).ShouldBeNull();

        mock.Verify(m => m.Invoke(It.IsAny<DataFlow>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenDataFlowIsLeased_ShouldReturnFailure()
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

        var mock = new Mock<Func<DataFlow, StatusResult<DataFlow>>>();
        _sdk.OnStart += mock.Object;

        var result = await _service.StartAsync(startMessage);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(Conflict);
        mock.Verify(m => m.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task StartByIdAsync_WhenExists_ShouldReturnSuccess()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Uninitialized);
        flow.IsConsumer = true;
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var msg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["key1"] = "value1" }
            }
        };

        var result = await _service.StartByIdAsync(flow.Id, msg);
        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task StartByIdAsync_WhenNotExists_ShouldReturnFailure()
    {
        var msg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["key1"] = "value1" }
            }
        };

        var result = await _service.StartByIdAsync("not-exist", msg);
        result.IsFailed.ShouldBeTrue();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(NotFound);
    }

    [Fact]
    public async Task StartByIdAsync_WhenNotConsumer_ShouldReturnFailure()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Uninitialized);
        flow.IsConsumer = false;
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var msg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["key1"] = "value1" }
            }
        };

        var result = await _service.StartByIdAsync(flow.Id, msg);
        result.IsFailed.ShouldBeTrue();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(Conflict);
    }

    [Fact]
    public async Task StartByIdAsync_WhenWrongState_ShouldReturnFailure()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Completed); // can't start from Terminated
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var msg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["key1"] = "value1" }
            }
        };

        var result = await _service.StartByIdAsync(flow.Id, msg);
        result.IsFailed.ShouldBeTrue();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(Conflict);
    }

    [Fact]
    public async Task StartByIdAsync_WhenAlreadyStarted_ShouldReturnSuccess()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Started);
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var msg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["key1"] = "value1" }
            }
        };

        var result = await _service.StartByIdAsync(flow.Id, msg);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
    }

    [Fact]
    public async Task Prepare_ShouldReturnSuccess_WhenNotExists()
    {
        var msg = CreatePrepareMessage();
        var provisionMock = new Mock<Func<DataFlow, StatusResult<DataFlow>>>();
        provisionMock.Setup(m => m.Invoke(It.IsAny<DataFlow>()))
            .Returns((DataFlow df) =>
            {
                df.AddResourceDefinitions([
                    new ProvisionResource
                    {
                        Flow = "flow-id",
                        Type = "test-type",
                        DataAddress = new DataAddress("test-type")
                    }
                ]);
                df.State = Prepared;
                return StatusResult<DataFlow>.Success(df);
            });


        _sdk.OnPrepare = provisionMock.Object;
        var result = await _service.PrepareAsync(msg);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        _dataFlowContext.DataFlows.ShouldContain(x => x.ResourceDefinitions.Count == 1 &&
                                                      x.State == Prepared);
        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
    }

    [Fact]
    public async Task Prepare_ShouldReturnSuccess_WhenAlreadyExists()
    {
        var flow = CreateDataFlow("flow-id1", Prepared);
        _dataFlowContext.DataFlows.Add(flow);
        await _dataFlowContext.SaveChangesAsync();

        var msg = CreatePrepareMessage();

        var provisionMock = new Mock<Func<DataFlow, StatusResult<DataFlow>>>();
        provisionMock.Setup(m => m.Invoke(It.IsAny<DataFlow>()))
            .Returns((DataFlow df) =>
            {
                df.AddResourceDefinitions([
                    new ProvisionResource
                    {
                        Flow = "flow-id",
                        Type = "another type",
                        DataAddress = new DataAddress("some data address type")
                    }
                ]);
                df.State = Prepared;
                return StatusResult<DataFlow>.Success(df);
            });

        _sdk.OnPrepare = provisionMock.Object;

        msg.ProcessId = flow.Id;
        var result = await _service.PrepareAsync(msg);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        _dataFlowContext.DataFlows.ShouldContain(x => x.ResourceDefinitions.Count == 1 &&
                                                      x.State == Prepared &&
                                                      x.ResourceDefinitions.Any(pr => pr.Type == "another type"));
    }

    [Fact]
    public async Task Prepare_ShouldReturnFailure_WhenSdkReportsFailure()
    {
        var msg = CreatePrepareMessage();
        var provisionMock = new Mock<Func<DataFlow, StatusResult<DataFlow>>>();
        provisionMock.Setup(m => m.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult<DataFlow>.FromCode(0, "test-error"));
        _sdk.OnPrepare = provisionMock.Object;
        var result = await _service.PrepareAsync(msg);
        result.IsSucceeded.ShouldBeFalse();
        result.Content.ShouldBeNull();
        result.Failure.ShouldNotBeNull();
        result.Failure.Message.ShouldBe("test-error");
        _dataFlowContext.DataFlows.ShouldBeEmpty();
        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
    }

    [Fact]
    public async Task Prepare_ShouldMoveToNotified_WhenNoResources()
    {
        var msg = CreatePrepareMessage();
        var result = await _service.PrepareAsync(msg);
        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
        _dataFlowContext.DataFlows.ShouldContain(x => x.ResourceDefinitions.Count == 0 &&
                                                      x.State == Prepared);
        _dataFlowContext.ChangeTracker.HasChanges().ShouldBeFalse();
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

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult.Success());

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

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult.Conflict("foobartestmessage"));

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

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task TerminateAsync_ShouldDeprovision_WhenInProvisioned()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Prepared;

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var result = await _service.TerminateAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        _dataFlowContext.DataFlows.ShouldContain(x => x.Id == dataFlowId && x.State == Terminated);
    }

    [Fact]
    public async Task SuspendAsync_ShouldReturnSuccess_WhenDataFlowExists()
    {
        const string dataFlowId = "test-flow-id";
        const string reason = "Test Suspend";
        var dataFlow = CreateDataFlow(dataFlowId);
        dataFlow.State = Started;
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
        dataFlow.State = Started;

        _dataFlowContext.DataFlows.Add(dataFlow);
        await _dataFlowContext.SaveChangesAsync();

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult.Success());

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

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        eventMock.Setup(f => f.Invoke(It.IsAny<DataFlow>()))
            .Returns(StatusResult.Conflict("foobartestmessage"));

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

        var eventMock = new Mock<Func<DataFlow, StatusResult>>();
        _sdk.OnSuspend += eventMock.Object;

        var result = await _service.SuspendAsync(dataFlowId, reason);

        result.IsSucceeded.ShouldBeTrue();
        eventMock.Verify(ev => ev.Invoke(dataFlow), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_WhenFound_ShouldReturnSuccess()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Started);
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var res = await _service.CompleteAsync(flow.Id);
        res.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task CompleteAsync_WhenWrongState_ShouldReturnConflict()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Started);
        flow.State = Terminated;

        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        var res = await _service.CompleteAsync(flow.Id);
        res.IsSucceeded.ShouldBeFalse();
        res.Failure.ShouldNotBeNull();
        res.Failure.Reason.ShouldBe(Conflict);
    }

    [Fact]
    public async Task CompleteAsync_WhenSdkReportsError_ShouldReturnError()
    {
        var flow = CreateDataFlow(Guid.NewGuid().ToString(), Started);
        await _dataFlowContext.AddAsync(flow);
        await _dataFlowContext.SaveChangesAsync();

        _sdk.OnComplete = _ => StatusResult.Failed(new StatusFailure { Message = "test error", Reason = InternalError });

        var res = await _service.CompleteAsync(flow.Id);
        res.IsSucceeded.ShouldBeFalse();
        res.Failure.ShouldNotBeNull();
        res.Failure.Reason.ShouldBe(InternalError);
        res.Failure.Message.ShouldBe("test error");
    }

    [Fact]
    public async Task CompleteAsync_WhenNotFound_ShouldReturnNotFound()
    {
        var res = await _service.CompleteAsync("not-exist");
        res.IsSucceeded.ShouldBeFalse();
        res.Failure.ShouldNotBeNull();
        res.Failure.Reason.ShouldBe(NotFound);
    }
}

[CollectionDefinition("SignalingService")] //parallelize tests in this collection
public class TestCollection;

[Collection("SignalingService")]
public class InMemDataPlaneSignalingServiceTest : DataPlaneSignalingServiceTest
{
    public InMemDataPlaneSignalingServiceTest()
    {
        var ctx = CreateInMem("test-lock-id");
        Initialize(ctx);
    }
}

[Collection("SignalingService")]
public class PostgresDataPlaneSignalingServiceTest : DataPlaneSignalingServiceTest, IAsyncDisposable
{
    private static PostgreSqlContainer? _postgreSqlContainer;

    public PostgresDataPlaneSignalingServiceTest()
    {
        const string dbName = "SdkApiTests";
        if (_postgreSqlContainer == null) // create only once per test run
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithDatabase(dbName)
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithPortBinding(5432, true)
                .Build();
            _postgreSqlContainer.StartAsync().Wait();
        }

        var port = _postgreSqlContainer.GetMappedPublicPort(5432);
        // dynamically map port to avoid conflicts
        var ctx = CreatePostgres($"Host=localhost;Port={port};Database={dbName};Username=postgres;Password=postgres", "test-lock-id");
        ctx.Database.EnsureCreated();
        Initialize(ctx);
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