using Sdk.Core.Domain.Messages;

namespace Sdk.Core.Domain;

public interface IDataplaneSignalingService
{
    /// <summary>
    /// Starts a data flow by sending a DataflowStartMessage to the data plane signaling service.
    /// </summary>
    /// <param name="message">The start message/></param>
    /// <returns>A status result that contains the response message if successful</returns>
    Task<StatusResult<DataFlowResponseMessage>> Start(DataflowStartMessage message); 
}