using Sdk.Core.Domain.Model;

namespace Sdk.Core.Domain.Interfaces;

public interface IControlApiClient
{
    /// <summary>
    ///     Registers a data plane instance with the control plane.
    /// </summary>
    /// <param name="dataplaneInstance">The instance - contains self-description of the data plane</param>
    /// <returns>An <see cref="IdResponse" /> if the data plane was registered successfully</returns>
    Task<IdResponse> RegisterDataPlane(DataPlaneInstance dataplaneInstance);

    /// <summary>
    ///     Unregisters a data plane instance from the control plane.
    /// </summary>
    /// <param name="dataPlaneInstanceId">The ID of the data plane</param>
    Task UnregisterDataPlane(string dataPlaneInstanceId);
}