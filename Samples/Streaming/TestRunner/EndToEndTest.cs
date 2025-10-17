using System.Text.Json;
using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;
using Shouldly;

namespace TestRunner;

public class EndToEndTest
{
    private const string ConsumerHost = "http://localhost:8081";
    private const string ProviderHost = "http://localhost:8080";
    private const string ParticipantId = "dataplane-signaling-api";

    private readonly ControlPlaneSimulator _sim = new()
    {
        ConsumerHost = ConsumerHost,
        ProviderHost = ProviderHost,
        ConsumerParticipant = ParticipantId,
        ProviderParticipant = ParticipantId // same participant id for both for the sake of simplicity
    };

    // same participant id for both for the sake of simplicity

    [Fact]
    public async Task StreamingTest()
    {
        var accessToken = await ObtainAccessToken("dataplane-signaling-api", "mpoTntIrYjsBqhqo0xuzqRUtCWQCWjG3");
        accessToken.ShouldNotBeNull();

        var client = new HttpClient();

        // prepare consumer
        var flowId = await _sim.PrepareConsumer(accessToken);

        // start provider
        var msg = await _sim.StartProvider(accessToken, flowId);
        msg.ShouldNotBeNull();
        msg.DataAddress.ShouldNotBeNull();
        msg.DataAddress.Properties.ShouldContainKey("endpoint");
        msg.DataAddress.Properties.ShouldContainKey("endpointProperties");

        var providerDa = msg.DataAddress;

        // notify consumer started
        var response = await _sim.NotifyConsumerStarted(accessToken, providerDa, flowId);
        response.ShouldNotBeNull();
        response.DataAddress.ShouldNotBeNull();
        response.State.ShouldBeEquivalentTo(DataFlowState.Started);

        //terminate provider
        await _sim.TerminateProvider(accessToken, flowId);
    }

    private async Task<string> ObtainAccessToken(string clientId, string clientSecret)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:8088");
        var entries = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret),
            new("scope", "profile email")
        };
        var result = await client.PostAsync("/realms/dataplane-signaling-api/protocol/openid-connect/token", new FormUrlEncodedContent(entries));

        var at = await JsonSerializer.DeserializeAsync<AccessTokenResponse>(await result.Content.ReadAsStreamAsync());


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