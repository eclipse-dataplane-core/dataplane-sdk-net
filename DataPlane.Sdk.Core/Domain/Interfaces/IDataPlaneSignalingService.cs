using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Interfaces;

public interface IDataPlaneSignalingService
{
    /// <summary>
    ///     Starts a data flow by sending a DataFlowStartMessage to the data plane signaling service.
    /// </summary>
    /// <param name="message">The start message/></param>
    /// <returns>A status result that contains the response message if successful</returns>
    Task<StatusResult<DataFlow>> StartAsync(DataFlowStartMessage message);

    /// <summary>
    ///     Suspends (pauses) a data flow by its ID.
    /// </summary>
    Task<StatusResult> SuspendAsync(string dataFlowId, string? reason = null);

    /// <summary>
    ///     Terminates (aborts) a data flow by its ID.
    /// </summary>
    /// <param name="dataFlowId">Data flow ID</param>
    /// <param name="reason">Optional reason</param>
    Task<StatusResult> TerminateAsync(string dataFlowId, string? reason = null);

    /// <summary>
    ///     Returns the transfer state for the process.
    /// </summary>
    /// <param name="processId"></param>
    Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId);

    /// <summary>
    ///     Validate the start message, i.e. check if the data flow already exists, if source and destination addresses are
    ///     valid, etc.
    /// </summary>
    /// <param name="startMessage"></param>
    Task<StatusResult> ValidateStartMessageAsync(DataFlowStartMessage startMessage);

    //todo: add restart flows, resourceProvisioned, resourceDeprovisioned, etc.

    /// <summary>
    ///     Initialize the preparation phase of the data transmission.
    /// </summary>
    /// <param name="prepareMessage"></param>
    /// <returns>
    ///     A DataFlow that reflects the current state: if the current state is PREPARING, then the caller must assume the
    ///     preparation to happen asynchronously. If the state is PREPARED, then the caller can proceed normally.
    /// </returns>
    Task<StatusResult<DataFlow>> PrepareAsync(DataFlowPrepareMessage prepareMessage);
}