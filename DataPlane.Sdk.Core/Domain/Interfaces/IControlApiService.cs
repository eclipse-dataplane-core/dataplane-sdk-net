using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Interfaces;

[Obsolete("This interface is deprecated and will be removed in a future version.")]
public interface IControlApiService
{
    /// <summary>
    ///     Registers a data plane instance with the control plane.
    /// </summary>
    /// <param name="dataplaneInstance">The instance - contains self-description of the data plane</param>
    /// <returns>An <see cref="IdResponse" /> if the data plane was registered successfully</returns>
    Task<StatusResult<IdResponse>> RegisterDataPlane(DataPlaneInstance dataplaneInstance);

    /// <summary>
    ///     Unregisters a data plane instance from the control plane.
    /// </summary>
    /// <param name="dataPlaneId">The ID of the data plane</param>
    Task<StatusResult> UnregisterDataPlane(string dataPlaneId);

    /// <summary>
    ///     Deletes the specified data plane instance.
    /// </summary>
    /// <param name="dataPlaneId">The unique identifier of the data plane instance to delete.</param>
    Task<StatusResult> DeleteDataPlane(string dataPlaneId);
}