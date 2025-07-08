using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using Shouldly;
using static Sdk.Api.Test.TestAuthHandler;

namespace Sdk.Api.Test;

public class DataPlaneSignalingApiControllerTest
{
    private readonly DataFlowContext _dataFlowContext;
    private readonly HttpClient _htmlClient;

    public DataPlaneSignalingApiControllerTest()
    {
        var sdk = new DataPlaneSdk();
        _dataFlowContext = DataFlowContextFactory.CreateInMem("test-leaser");
        var dataPlaneSignalingService = new DataPlaneSignalingService(_dataFlowContext, sdk, "test-runtime-id");

        // need to wire those two up here, because we need access to the db context
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDataPlaneStore>(_dataFlowContext);
                services.AddSingleton<IDataPlaneSignalingService>(dataPlaneSignalingService);
            });
        });
        _htmlClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetState_Success()
    {
        await _dataFlowContext.DataFlows.AddAsync(CreateDataFlow());
        await _dataFlowContext.SaveChangesAsync();
        var response = await _htmlClient.GetAsync($"/api/v1/{TestUser}/dataflows/flow1/state");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetState_WrongParticipantInUrlPath()
    {
        await _dataFlowContext.DataFlows.AddAsync(CreateDataFlow());
        await _dataFlowContext.SaveChangesAsync();
        var response = await _htmlClient.GetAsync("/api/v1/invalid-participant/dataflows/flow1/state");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetState_DoesNotOwnDataFlow()
    {
        await _dataFlowContext.DataFlows.AddAsync(CreateDataFlow(participantId: "another-user"));
        await _dataFlowContext.SaveChangesAsync();
        var response = await _htmlClient.GetAsync($"/api/v1/{TestUser}/dataflows/flow1/state");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetState_DataFlowNotFound()
    {
        await _dataFlowContext.DataFlows.AddAsync(CreateDataFlow("another-flow"));
        await _dataFlowContext.SaveChangesAsync();
        var response = await _htmlClient.GetAsync($"/api/v1/{TestUser}/dataflows/flow1/state");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private DataFlow CreateDataFlow(string id = "flow1", string participantId = TestUser)
    {
        return new DataFlow(id)
        {
            Source = new DataAddress("test-type"),
            Destination = new DataAddress("test-type"),
            TransferType = new TransferType("test-type", FlowType.Pull),
            RuntimeId = "test-runtime-id",
            ParticipantId = participantId,
            AssetId = "test-asset",
            AgreementId = "test-agreement",
            State = DataFlowState.Notified
        };
    }
}