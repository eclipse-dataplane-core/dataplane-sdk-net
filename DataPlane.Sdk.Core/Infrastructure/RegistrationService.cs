using System.Net.Http.Json;
using DataPlane.Sdk.Core.Domain;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Microsoft.Extensions.Options;

namespace DataPlane.Sdk.Core.Infrastructure;

/// <summary>
///     Client service to communicate with an EDC Control Plane's Control API.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;


    public RegistrationService(IHttpClientFactory factory, IOptions<ControlApiOptions> options)
    {
        _baseUrl = options.Value.BaseUrl ?? throw new ArgumentException("BaseUrl must be set in ControlApiOptions");
        // must use named HTTP client to make use of all configuration etc.
        _httpClient = factory.CreateClient(IConstants.HttpClientName);
    }

    public async Task<StatusResult<IdResponse>> RegisterDataPlane(DataPlaneInstance dataplaneInstance)
    {
        if (dataplaneInstance.AllowedSourceTypes.Count == 0)
        {
            // todo: return a result here?
            throw new ArgumentException("Must specify at least one allowed source type");
        }

        if (dataplaneInstance.AllowedTransferTypes.Count == 0)
        {
            // todo: return a result here?
            throw new ArgumentException("Must specify at least one allowed transfer type");
        }

        var result = await _httpClient.PostAsJsonAsync(_baseUrl + "/v1/dataplanes", new DataPlaneDto(dataplaneInstance));

        if (result.IsSuccessStatusCode)
        {
            var r = await result.Content.ReadFromJsonAsync<IdResponse>();
            return StatusResult<IdResponse>.Success(r);
        }

        return StatusResult<IdResponse>.FromCode((int)result.StatusCode, result.ReasonPhrase);
    }

    public async Task<StatusResult> UnregisterDataPlane(string dataPlaneId)
    {
        var result = await _httpClient.PutAsync($"{_baseUrl}/v1/dataplanes/{dataPlaneId}/unregister", null);
        return Response(result);
    }

    public async Task<StatusResult> DeleteDataPlane(string dataPlaneId)
    {
        var result = await _httpClient.DeleteAsync($"{_baseUrl}/v1/dataplanes/{dataPlaneId}");
        return Response(result);
    }

    private static StatusResult Response(HttpResponseMessage result)
    {
        return result.IsSuccessStatusCode
            ? StatusResult.Success()
            : StatusResult.FromCode((int)result.StatusCode, result.ReasonPhrase);
    }
}

public class ControlApiOptions
{
    public required string BaseUrl { get; init; }
}