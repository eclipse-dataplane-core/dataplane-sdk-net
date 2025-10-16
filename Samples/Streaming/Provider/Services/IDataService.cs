using DataPlane.Sdk.Core.Domain.Model;

namespace Provider.Services;

/// <summary>
///     Represents a service for managing data flows, permissions, and public endpoint configuration.
/// </summary>
public interface IDataService
{
    /// <summary>
    ///     Determines whether the specified API key has permission to access the given data flow.
    /// </summary>
    /// <param name="apiKey">The API key to be checked for permissions.</param>
    /// <param name="dataFlow">The data flow object for which the permission is being verified.</param>
    /// <returns>
    ///     Returns a task representing the asynchronous operation, containing a boolean value that indicates whether the
    ///     permission is granted.
    /// </returns>
    Task<bool> IsPermitted(string apiKey, DataFlow dataFlow);

    /// <summary>
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    StatusResult<DataFlow> ProcessStart(DataFlow flow);

    Task<StatusResult> ProcessTerminate(DataFlow dataFlow);
}