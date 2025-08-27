using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Interfaces;

public interface IControlPlaneSignalingClient
{
    /// <summary>
    ///     Notify the control plane that a data flow has finished preparing.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult<Void>> NotifyPrepared(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has started.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult<Void>> NotifyStarted(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has completed normally.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult<Void>> NotifyCompleted(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has errored out and is now terminated.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    /// <param name="reason">The reason for the error</param>
    Task<StatusResult<Void>> NotifyErrored(string dataFlowId, string reason);
}