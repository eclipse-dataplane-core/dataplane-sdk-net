using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;
using Polly;
using Polly.Wrap;
using Shouldly;

namespace TestRunner;

public class EndToEndTest
{
    private const string ConsumerHost = "http://localhost:8081";
    private const string ProviderHost = "http://localhost:8080";
    private const string ParticipantId = "dataplane-signaling-api";
    private const string KeycloakHost = "http://localhost:8088";


    private readonly AsyncPolicyWrap _pollyRetry;

    private readonly ControlPlaneSimulator _sim = new()
    {
        ConsumerHost = ConsumerHost,
        ProviderHost = ProviderHost,
        ConsumerParticipant = ParticipantId,
        ProviderParticipant = ParticipantId // same participant id for both for the sake of simplicity
    };

    public EndToEndTest()
    {
        var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(5));
        var retry = Policy.Handle<Exception>().WaitAndRetryForeverAsync(_ => TimeSpan.FromMilliseconds(500));
        _pollyRetry = Policy.WrapAsync(timeout, retry);
    }


    [Fact]
    public async Task StreamingTest()
    {
        var accessToken = await _pollyRetry
            .ExecuteAsync(async () => await ObtainAccessToken("dataplane-signaling-api", "mpoTntIrYjsBqhqo0xuzqRUtCWQCWjG3"));

        accessToken.ShouldNotBeNull();

        // prepare consumer
        var flowId = await _sim.PrepareConsumer(accessToken);
        await Task.Delay(1000);

        // start provider
        var msg = await _sim.StartProvider(accessToken, flowId);
        msg.ShouldNotBeNull();
        msg.DataAddress.ShouldNotBeNull();
        msg.DataAddress.Properties.ShouldContainKey("endpoint");
        msg.DataAddress.Properties.ShouldContainKey("endpointProperties");

        var providerDa = msg.DataAddress;
        await Task.Delay(1000);

        // notify consumer started
        var response = await _sim.NotifyConsumerStarted(accessToken, providerDa, flowId);
        response.ShouldNotBeNull();
        response.DataAddress.ShouldNotBeNull();
        response.State.ShouldBeEquivalentTo(DataFlowState.Started);
        await Task.Delay(1000);

        //terminate provider
        await _sim.TerminateProvider(accessToken, flowId);
    }

    private async Task<string> ObtainAccessToken(string clientId, string clientSecret)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(KeycloakHost);
        var entries = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret),
            new("scope", "profile email")
        };
        var result = await client.PostAsync("/realms/dataplane-signaling-api/protocol/openid-connect/token", new FormUrlEncodedContent(entries));

        var at = await result.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return at != null ? at.AccessToken : throw new Exception("Failed to obtain access token");
    }

    internal class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public required int RefreshExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }
    }
}