using System.Net.Http.Json;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace TestRunner;

public class ControlPlaneSimulator
{
    private readonly HttpClient _httpClient = new();

    public required string ConsumerHost { get; set; }
    public required string ProviderHost { get; set; }
    public required string ConsumerParticipant { get; set; }
    public required string ProviderParticipant { get; set; }

    public async Task<string> PrepareConsumer(string accessToken)
    {
        var flowId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ConsumerHost}/api/v1/dataplane-signaling-api/dataflows/prepare");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var body = new DataFlowPrepareMessage
        {
            ParticipantId = ConsumerParticipant,
            ProcessId = flowId,
            AgreementId = "test-agreement",
            DatasetId = "test-asset",
            TransferType = new TransferType
            {
                DestinationType = "NatsStream",
                FlowType = FlowType.Pull
            }
        };

        request.Content = JsonContent.Create(body);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return flowId;
    }

    public async Task<DataFlowResponseMessage?> StartProvider(string accessToken, string flowId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ProviderHost}/api/v1/{ProviderParticipant}/dataflows/start");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var spRqBody = new DataFlowStartMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            ParticipantId = ProviderParticipant,
            CounterPartyId = "consumer",
            DataspaceContext = "dataspace-1",
            ProcessId = flowId,
            AgreementId = "test-agreement",
            DatasetId = "test-asset",
            CallbackAddress = new Uri("https://example.com/callback"),
            TransferType = new TransferType
            {
                DestinationType = "NatsStream",
                FlowType = FlowType.Pull
            }
        };
        request.Content = JsonContent.Create(spRqBody);
        var spResponse = await _httpClient.SendAsync(request);

        spResponse.EnsureSuccessStatusCode();
        return await spResponse.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
    }

    public async Task<DataFlowResponseMessage?> NotifyConsumerStarted(string accessToken, DataAddress providerDa, string flowId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ConsumerHost}/api/v1/{ConsumerParticipant}/dataflows/{flowId}/started");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var body = new DataFlowStartedNotificationMessage
        {
            DataAddress = providerDa
        };
        request.Content = JsonContent.Create(body);
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<DataFlowResponseMessage>();
    }

    public async Task TerminateProvider(string accessToken, string flowId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ConsumerHost}/api/v1/{ConsumerParticipant}/dataflows/{flowId}/terminate");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var body = new DataFlowTerminateMessage
        {
            Reason = "normal termination"
        };
        request.Content = JsonContent.Create(body);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}