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
    ///     Starts a data flow that already exists.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message">The start message</param>
    Task<StatusResult<DataFlow>> StartByIdAsync(string id, DataFlowStartByIdMessage message);

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
    ///     Initialize the preparation phase of the data transmission.
    /// </summary>
    /// <param name="prepareMessage"></param>
    /// <returns>
    ///     A DataFlow that reflects the current state: if the current state is PREPARING, then the caller must assume the
    ///     preparation to happen asynchronously. If the state is PREPARED, then the caller can proceed normally.
    /// </returns>
    Task<StatusResult<DataFlow>> PrepareAsync(DataFlowPrepareMessage prepareMessage);

    /// <summary>
    ///     Marks a data flow as completed.
    /// </summary>
    /// <param name="dataFlowId">The ID of the data flow to complete</param>
    /// <returns>
    ///     A status result indicating success or failure. Failure may include specific details
    ///     such as wrong state, not found, or other error conditions.
    /// </returns>
    Task<StatusResult> CompleteAsync(string dataFlowId);
}