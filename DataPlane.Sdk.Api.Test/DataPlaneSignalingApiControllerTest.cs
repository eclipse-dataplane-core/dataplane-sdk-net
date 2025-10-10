using System.Net;
using System.Net.Http.Json;
using DataPlane.Sdk.Api.Test.Fixtures;
using DataPlane.Sdk.Core;
using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Shouldly;
using static DataPlane.Sdk.Api.Test.TestAuthHandler;


namespace DataPlane.Sdk.Api.Test;

/// <summary>
///     Base class for DPS API controller tests
/// </summary>
public abstract class DataPlaneSignalingApiControllerTest(DataFlowContext dataFlowContext, HttpClient httpClient, DataPlaneSdk sdk) : IDisposable
{
    private DataFlowContext DataFlowContext { get; } = dataFlowContext;
    private HttpClient HttpClient { get; } = httpClient;
    private DataPlaneSdk Sdk { get; } = sdk;

    public void Dispose()
    {
        DataFlowContext.DataFlows.RemoveRange(DataFlowContext.DataFlows);
        DataFlowContext.Leases.RemoveRange(DataFlowContext.Leases);
        DataFlowContext.SaveChanges();
    }

    #region Suspend

    [Fact]
    public async Task Suspend_Success()
    {
        Sdk.OnSuspend = null;
        var dataFlow = CreateDataFlow();
        dataFlow.State = DataFlowState.Started;
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/suspend", new DataFlowSuspendMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Suspend_Notfound_Expect404()
    {
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/not-exist/suspend", new DataFlowSuspendMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Suspend_InWrongState_Expect400()
    {
        Sdk.OnSuspend = null;
        var dataFlow = CreateDataFlow();
        dataFlow.State = DataFlowState.Preparing; // cannot transition from preparing to suspended
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/suspend", new DataFlowSuspendMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Suspend_AlreadySuspended_Expect200()
    {
        Sdk.OnSuspend = null;
        var dataFlow = CreateDataFlow();
        dataFlow.State = DataFlowState.Suspended; // should be a noop
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/suspend", new DataFlowSuspendMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Terminate

    [Fact]
    public async Task Terminate_Success()
    {
        Sdk.OnTerminate = null;
        var dataFlow = CreateDataFlow();
        dataFlow.State = DataFlowState.Started;
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/terminate", new DataFlowTerminateMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Terminate_Notfound_Expect404()
    {
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/not-exist/terminate", new DataFlowTerminateMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Terminate_AlreadyTerminated_Expect200()
    {
        Sdk.OnSuspend = null;
        var dataFlow = CreateDataFlow();
        dataFlow.State = DataFlowState.Terminated; // should be a noop
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/terminate", new DataFlowSuspendMessage
        {
            Reason = "test reason"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Prepare

    [Fact]
    public async Task Prepare_Success()
    {
        Sdk.OnPrepare = null;
        var prepareMsg = new DataFlowPrepareMessage
        {
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };
        var jsonContent = JsonContent.Create(prepareMsg);
        var response = await HttpClient.PostAsync($"/api/v1/{TestUser}/dataflows/prepare", jsonContent);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Prepared);
        body.DataAddress.ShouldBeNull();
    }

    [Fact]
    public async Task Prepare_WhenReturnsSync_Success()
    {
        Sdk.OnPrepare = flow =>
        {
            flow.State = DataFlowState.Prepared;
            return StatusResult<DataFlow>.Success(flow);
        };
        var prepareMsg = new DataFlowPrepareMessage
        {
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };
        var jsonContent = JsonContent.Create(prepareMsg);
        var response = await HttpClient.PostAsync($"/api/v1/{TestUser}/dataflows/prepare", jsonContent);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Location.ShouldBeNull();
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Prepared);
        body.DataAddress.ShouldBeNull();
    }

    [Fact]
    public async Task Prepare_WhenReturnsAsync_Success()
    {
        Sdk.OnPrepare = dataFlow =>
        {
            dataFlow.State = DataFlowState.Preparing;
            return StatusResult<DataFlow>.Success(dataFlow);
        };
        var prepareMsg = new DataFlowPrepareMessage
        {
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };
        var jsonContent = JsonContent.Create(prepareMsg);
        var response = await HttpClient.PostAsync($"/api/v1/{TestUser}/dataflows/prepare", jsonContent);
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldEndWith($"/api/v1/{TestUser}/dataflows/test-process");

        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Preparing);
        body.DataAddress.ShouldBeNull();
    }

    [Fact]
    public async Task Prepare_WhenSdkReturnsInvalidState_Expect400()
    {
        Sdk.OnPrepare = dataFlow =>
        {
            dataFlow.State = DataFlowState.Completed;
            return StatusResult<DataFlow>.Success(dataFlow);
        };
        var prepareMsg = new DataFlowPrepareMessage
        {
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };
        var jsonContent = JsonContent.Create(prepareMsg);
        var response = await HttpClient.PostAsync($"/api/v1/{TestUser}/dataflows/prepare", jsonContent);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Prepare_WhenDataflowExists_Expect409()
    {
        var dataFlow = CreateDataFlow();
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var prepareMsg = new DataFlowPrepareMessage
        {
            ProcessId = dataFlow.Id,
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };
        var jsonContent = JsonContent.Create(prepareMsg);
        var response = await HttpClient.PostAsync($"/api/v1/{TestUser}/dataflows/prepare", jsonContent);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    private static DataFlow CreateDataFlow(string? id = null, string participantId = TestUser)
    {
        id ??= Guid.NewGuid().ToString();

        return new DataFlow(id)
        {
            Source = new DataAddress("test-type"),
            Destination = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            },
            RuntimeId = "test-runtime-id",
            ParticipantId = participantId,
            AssetId = "test-asset",
            AgreementId = "test-agreement",
            State = DataFlowState.Uninitialized
        };
    }

    #endregion

    #region GetState

    [Fact]
    public async Task GetState_Success()
    {
        var dataFlow = CreateDataFlow();
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.GetAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/status");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetState_WrongParticipantInUrlPath()
    {
        var dataFlow = CreateDataFlow();
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.GetAsync($"/api/v1/invalid-participant/dataflows/{dataFlow.Id}/status");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetState_DoesNotOwnDataFlow()
    {
        var dataFlow = CreateDataFlow(participantId: "another-user");
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.GetAsync($"/api/v1/{TestUser}/dataflows/{dataFlow.Id}/status");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetState_DataFlowNotFound()
    {
        await DataFlowContext.DataFlows.AddAsync(CreateDataFlow("another-flow"));
        await DataFlowContext.SaveChangesAsync();
        var response = await HttpClient.GetAsync($"/api/v1/{TestUser}/dataflows/not-exist/status");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region Start

    [Fact]
    public async Task Start_Success()
    {
        Sdk.OnStart = null;

        var id = Guid.NewGuid().ToString();
        var message = new DataFlowStartMessage
        {
            MessageId = id,
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            DestinationDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/start", message);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Started);
        body.DataAddress.ShouldNotBeNull();
    }

    [Fact]
    public async Task Start_WhenAlreadyExists_ExpectConflict()
    {
        var dataFlow = CreateDataFlow();
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var message = new DataFlowStartMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            ProcessId = dataFlow.Id,
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            DestinationDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/start", message);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Start_SdkHandlerWrongState_ExpectBadRequest()
    {
        Sdk.OnStart = dataFlow =>
        {
            dataFlow.State = DataFlowState.Completed;
            return StatusResult<DataFlow>.Success(dataFlow);
        };

        var id = Guid.NewGuid().ToString();
        var message = new DataFlowStartMessage
        {
            MessageId = id,
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            DestinationDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/start", message);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Start_WhenSdkReturnsStarted_Success()
    {
        Sdk.OnStart = dataFlow =>
        {
            dataFlow.State = DataFlowState.Started;
            return StatusResult<DataFlow>.Success(dataFlow);
        };
        var dataFlow = CreateDataFlow();
        await DataFlowContext.DataFlows.AddAsync(dataFlow);
        await DataFlowContext.SaveChangesAsync();

        var message = new DataFlowStartMessage
        {
            MessageId = dataFlow.Id,
            ProcessId = "test-process",
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            SourceDataAddress = new DataAddress("test-type"),
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/start", message);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
    }

    [Fact]
    public async Task Start_WhenSdkReturnsStarting_Success()
    {
        Sdk.OnStart = dataFlow =>
        {
            dataFlow.State = DataFlowState.Starting;
            return StatusResult<DataFlow>.Success(dataFlow);
        };

        var id = Guid.NewGuid().ToString();
        var message = new DataFlowStartMessage
        {
            ProcessId = id,
            DatasetId = "test-asset",
            ParticipantId = TestUser,
            AgreementId = "test-agreement",
            SourceDataAddress = new DataAddress("test-type"),
            DestinationDataAddress = new DataAddress("test-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/start", message);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.Headers.Location.ShouldNotBeNull()
            .ToString().ShouldEndWith($"/api/v1/{TestUser}/dataflows/{id}");
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Starting);
        body.DataAddress.ShouldNotBeNull();
    }

    #endregion

    #region StartById

    [Fact]
    public async Task StartById_Success()
    {
        sdk.OnStart = null;
        var flow = CreateDataFlow();
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{flow.Id}/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Started); // default behavior
        body.DataAddress.ShouldNotBeNull();
    }

    [Fact]
    public async Task StartById_WhenNotFound_ExpectError()
    {
        sdk.OnStart = null;

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/not-exist/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartById_InvalidState_ExpectConflict()
    {
        sdk.OnStart = null;
        var flow = CreateDataFlow();
        flow.State = DataFlowState.Completed; // invalid state
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{flow.Id}/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task StartById_WhenSdkReturnsStarting_Success()
    {
        sdk.OnStart = df =>
        {
            df.State = DataFlowState.Starting;
            return StatusResult<DataFlow>.Success(df);
        };
        var flow = CreateDataFlow();
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{flow.Id}/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.Headers.Location.ShouldNotBeNull()
            .ToString().ShouldEndWith($"/api/v1/{TestUser}/dataflows/{flow.Id}");
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Starting); // default behavior
        body.DataAddress.ShouldNotBeNull();
    }

    [Fact]
    public async Task StartById_WhenSdkReturnsStarted_Success()
    {
        sdk.OnStart = df =>
        {
            df.State = DataFlowState.Started;
            return StatusResult<DataFlow>.Success(df);
        };
        var flow = CreateDataFlow();
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{flow.Id}/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
        body.ShouldNotBeNull();
        body.State.ShouldBe(DataFlowState.Started); // default behavior
        body.DataAddress.ShouldNotBeNull();
    }

    [Fact]
    public async Task StartById_SdkHandlerWrongState_ExpectBadRequest()
    {
        sdk.OnStart = df =>
        {
            df.State = DataFlowState.Suspended; // invalid state
            return StatusResult<DataFlow>.Success(df);
        };
        var flow = CreateDataFlow();
        DataFlowContext.DataFlows.Add(flow);
        await DataFlowContext.SaveChangesAsync();

        var startMsg = new DataFlowStartByIdMessage
        {
            SourceDataAddress = new DataAddress("test-type")
            {
                Properties = { ["test-key"] = "test-value" }
            }
        };
        var response = await HttpClient.PostAsJsonAsync($"/api/v1/{TestUser}/dataflows/{flow.Id}/start", startMsg);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    #endregion
}

/// <summary>
///     uses the in-memory db context
/// </summary>
public class InMem(InMemoryFixture fixture)
    : DataPlaneSignalingApiControllerTest(fixture.Context!, fixture.Client!, fixture.Sdk), IClassFixture<InMemoryFixture>;

/// <summary>
///     uses the PostgreSQL db context
/// </summary>
public class Postgres(PostgresFixture fixture)
    : DataPlaneSignalingApiControllerTest(fixture.Context!, fixture.Client!, fixture.Sdk), IClassFixture<PostgresFixture>;