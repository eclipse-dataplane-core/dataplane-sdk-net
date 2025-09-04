namespace HttpDataplane.Services;

public class HttpProxyService(HttpClient httpClient, ILogger<HttpProxyService> logger) : IHttpProxyService
{
    public async Task<string> GetData(string url)
    {
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        logger.LogError("Failed to get data from {Url}: {HttpResponseMessage}.", url, response);
        return response.ToString();
    }
}