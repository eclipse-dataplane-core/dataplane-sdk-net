using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Model;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Infrastructure;

public class ControlApiService : IControlApiService
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;


    public ControlApiService(IHttpClientFactory factory, IOptions<ControlApiOptions> options)
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

    public async Task<StatusResult<Void>> UnregisterDataPlane(string dataPlaneInstanceId)
    {
        var result = await _httpClient.PutAsync($"{_baseUrl}/v1/dataplanes/{dataPlaneInstanceId}/unregister", null);
        return Response(result);
    }

    public async Task<StatusResult<Void>> DeleteDataPlane(string dataPlaneInstanceId)
    {
        var result = await _httpClient.DeleteAsync($"{_baseUrl}/v1/dataplanes/{dataPlaneInstanceId}");
        return Response(result);
    }

    private static StatusResult<Void> Response(HttpResponseMessage result)
    {
        return result.IsSuccessStatusCode
            ? StatusResult<Void>.Success(default)
            : StatusResult<Void>.FromCode((int)result.StatusCode, result.ReasonPhrase);
    }
}

public class ControlApiOptions
{
    public string? BaseUrl { get; init; }
}