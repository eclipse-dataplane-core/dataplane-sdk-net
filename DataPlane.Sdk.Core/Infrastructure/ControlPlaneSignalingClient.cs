using System.Net.Http.Json;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Microsoft.Extensions.Options;

namespace DataPlane.Sdk.Core.Infrastructure;

public class ControlPlaneSignalingClient(HttpClient httpClient, IOptions<DataPlaneSdkOptions> sdkOptions) : IControlPlaneSignalingClient
{
    private readonly string _baseUrl = sdkOptions.Value.ControlApi.BaseUrl;
    private readonly string _dataplaneId = sdkOptions.Value.DataplaneId;

    public async Task<StatusResult> NotifyStarted(string dataFlowId)
    {
        return await SendRequest($"{_baseUrl}/transfers/{dataFlowId}/dataflow/started", CreateResponse());
    }

    public async Task<StatusResult> NotifyCompleted(string dataFlowId)
    {
        return await SendRequest($"{_baseUrl}/transfers/{dataFlowId}/dataflow/completed", new DataFlowCompletedMessage());
    }

    public async Task<StatusResult> NotifyErrored(string dataFlowId, string reason)
    {
        return await SendRequest($"{_baseUrl}/transfers/{dataFlowId}/dataflow/errored", new DataFlowErroredMessage
        {
            Reason = reason
        });
    }

    public async Task<StatusResult> NotifyPrepared(string dataFlowId)
    {
        return await SendRequest($"{_baseUrl}/transfers/{dataFlowId}/dataflow/prepared", CreateResponse());
    }

    private async Task<StatusResult> SendRequest(string url, object body)
    {
        var response = await httpClient.PostAsJsonAsync(url, body);
        return response.IsSuccessStatusCode
            ? StatusResult.Success()
            : StatusResult.FromCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }


    private DataFlowResponseMessage CreateResponse()
    {
        return new DataFlowResponseMessage
        {
            DataplaneId = _dataplaneId
        };
    }
}