using JetBrains.Annotations;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using Shouldly;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static System.Text.Json.JsonSerializer;

namespace Sdk.Core.Test.Infrastructure;

[TestSubject(typeof(ControlApiService))]
public class ControlApiServiceTest : IDisposable
{
    private readonly WireMockServer _mockServer;
    private readonly ControlApiService _service;
    private readonly string _testDataplaneId = "test-dataplane";

    public ControlApiServiceTest()
    {
        _mockServer = WireMockServer.Start(8083);
        _service = new ControlApiService(new HttpClient(), "http://localhost:8083/api/control");
    }

    public void Dispose()
    {
        _mockServer.Stop();
        _mockServer.Dispose();
    }

    [Fact]
    public async Task Register_Success()
    {
        _mockServer
            .Given(Request.Create().WithPath("/api/control/v1/dataplanes").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(IdResponseJson("test-dataplane")));
        var result = await _service.RegisterDataPlane(new DataPlaneInstance(_testDataplaneId)
        {
            Url = new Uri("http://localhost/dataplane"),
            State = DataPlaneState.Available,
            AllowedSourceTypes = ["test-source-type"],
            AllowedTransferTypes = ["test-transfer-type"]
        });

        result.IsSucceeded.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
    }

    private static string IdResponseJson(string id)
    {
        return Serialize(new IdResponse(id));
    }

    [Fact]
    public async Task Register_Failure()
    {
        _mockServer
            .Given(Request.Create().WithPath("/api/control/v1/dataplanes").UsingPost())
            .RespondWith(Response.Create().WithNotFound());
        var result = await _service.RegisterDataPlane(new DataPlaneInstance(_testDataplaneId)
        {
            Url = new Uri("http://localhost:8082"),
            State = DataPlaneState.Available,
            AllowedSourceTypes = ["test-source-type"],
            AllowedTransferTypes = ["test-transfer-type"]
        });

        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(FailureReason.NotFound);
    }

    [Fact]
    public async Task Register_MissingSourceOrTransferType()
    {
        await Should.ThrowAsync<ArgumentException>(async () =>
        {
            await _service.RegisterDataPlane(new DataPlaneInstance(_testDataplaneId)
            {
                Url = new Uri("http://localhost:8082"),
                State = DataPlaneState.Available
            });
        });
    }

    [Fact]
    public async Task UnregisterDataPlane_Success()
    {
        _mockServer
            .Given(Request.Create().WithPath($"/api/control/v1/dataplanes/{_testDataplaneId}/unregister").UsingPut())
            .RespondWith(Response.Create().WithSuccess());
        var result = await _service.UnregisterDataPlane(_testDataplaneId);
        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task UnregisterDataPlane_NotFound()
    {
        _mockServer
            .Given(Request.Create().WithPath($"/api/control/v1/dataplanes/{_testDataplaneId}/unregister").UsingPut())
            .RespondWith(Response.Create().WithNotFound());
        var result = await _service.UnregisterDataPlane(_testDataplaneId);
        result.IsSucceeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure.Reason.ShouldBe(FailureReason.NotFound);
    }

    
}