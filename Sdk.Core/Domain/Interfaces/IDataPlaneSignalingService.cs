using Sdk.Core.Domain.Messages;

namespace Sdk.Core.Domain.Interfaces;

public interface IDataPlaneSignalingService
{
    /// <summary>
    /// Starts a data flow by sending a DataflowStartMessage to the data plane signaling service.
    /// </summary>
    /// <param name="message">The start message/></param>
    /// <returns>A status result that contains the response message if successful</returns>
    Task<StatusResult<DataFlowResponseMessage>> StartAsync(DataflowStartMessage message); 
    
    /// <summary>
    /// Suspends (pauses) a data flow by its ID.
    /// </summary>
    Task<StatusResult<Void>> SuspendAsync(string dataFlowId);

    /// <summary>
    /// Terminates (aborts) a data flow by its ID.
    /// </summary>
    /// <param name="dataFlowId">Data flow ID</param>
    /// <param name="reason">Optional reason</param>
    Task<StatusResult<Void>> TerminateAsync(string dataFlowId, string? reason = null);

    /// <summary>
    /// Returns the transfer state for the process.
    /// </summary>
    /// <param name="processId"></param>
    Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId);
    
    //todo: add restart flows, resourceProvisioned, resourceDeprovisioned, etc.
}