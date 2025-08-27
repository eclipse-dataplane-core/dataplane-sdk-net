using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Interfaces;

/// <summary>
///     This interface should be used to communicate with the control plane, invoking the control plane's signaling API.
/// </summary>
public interface IControlPlaneSignalingClient
{
    /// <summary>
    ///     Notify the control plane that a data flow has finished preparing.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult> NotifyPrepared(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has started.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult> NotifyStarted(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has completed normally.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    Task<StatusResult> NotifyCompleted(string dataFlowId);

    /// <summary>
    ///     Notify the control plane that a data flow has errored out and is now terminated.
    /// </summary>
    /// <param name="dataFlowId">The dataflow's ID</param>
    /// <param name="reason">The reason for the error</param>
    Task<StatusResult> NotifyErrored(string dataFlowId, string reason);
}