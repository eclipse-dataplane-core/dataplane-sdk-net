namespace HttpDataplane.Services;

/// <summary>
///     Provides an interface for proxying HTTP requests and retrieving data from specified URLs.
/// </summary>
public interface IHttpProxyService
{
    /// <summary>
    ///     Retrieves data from the specified URL using an HTTP GET request.
    /// </summary>
    /// <param name="url">The URL from which data is to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response data as a string.</returns>
    public Task<string> GetData(string url);
}