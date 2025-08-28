using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using Microsoft.Extensions.Options;
using Shouldly;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Request = WireMock.RequestBuilders.Request;

namespace DataPlane.Sdk.Core.Test;

public class ControlPlaneSignalingClientTest
{
    private readonly WireMockServer _server;
    private readonly ControlPlaneSignalingClient _signalingClient;

    public ControlPlaneSignalingClientTest()
    {
        _server = WireMockServer.Start();
        var httpClient = _server.CreateClient();

        _signalingClient = new ControlPlaneSignalingClient(httpClient, Options.Create(new DataPlaneSdkOptions
        {
            RuntimeId = "test-runtime-id",
            ControlApi = new ControlApiOptions
            {
                BaseUrl = "http://localhost:" + _server.Port
            }
        }));
    }

    [Fact]
    public async Task NotifyCompleted()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/completed").UsingPost())
            .RespondWith(Response.Create().WithSuccess());
        var result = await _signalingClient.NotifyCompleted("test-flowid");

        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task NotifyCompleted_Failed()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/completed").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("test failure message"));
        var result = await _signalingClient.NotifyCompleted("test-flowid");

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Reason.ShouldBe(FailureReason.BadRequest);
        result.Failure!.Message.ShouldBe("test failure message");
    }

    [Fact]
    public async Task NotifyStarted()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/started").UsingPost())
            .RespondWith(Response.Create().WithSuccess());
        var result = await _signalingClient.NotifyStarted("test-flowid");

        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task NotifyStarted_Failed()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/started").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("test failure message"));
        var result = await _signalingClient.NotifyStarted("test-flowid");

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Reason.ShouldBe(FailureReason.BadRequest);
        result.Failure!.Message.ShouldBe("test failure message");
    }

    [Fact]
    public async Task NotifyPrepared()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/prepared").UsingPost())
            .RespondWith(Response.Create().WithSuccess());
        var result = await _signalingClient.NotifyPrepared("test-flowid");

        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task NotifyPrepared_Failed()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/prepared").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("test failure message"));
        var result = await _signalingClient.NotifyPrepared("test-flowid");

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Reason.ShouldBe(FailureReason.BadRequest);
        result.Failure!.Message.ShouldBe("test failure message");
    }

    [Fact]
    public async Task NotifyErrored()
    {
        const string expectedJson = """{"reason":"test reason"}""";

        _server.Given(Request.Create()
                .WithPath("/transfers/test-flowid/dataflow/errored")
                .UsingPost()
                .WithBodyAsJson(expectedJson))
            .RespondWith(Response.Create().WithSuccess());
        var result = await _signalingClient.NotifyErrored("test-flowid", "test reason");

        result.IsSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task NotifyErrored_Failed()
    {
        _server.Given(Request.Create().WithPath("/transfers/test-flowid/dataflow/errored").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("test failure message"));
        var result = await _signalingClient.NotifyErrored("test-flowid", "test reason");

        result.IsSucceeded.ShouldBeFalse();
        result.Failure!.Reason.ShouldBe(FailureReason.BadRequest);
        result.Failure!.Message.ShouldBe("test failure message");
    }
}