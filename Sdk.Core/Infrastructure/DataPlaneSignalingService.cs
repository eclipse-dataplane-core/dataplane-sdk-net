using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Infrastructure;

public class DataPlaneSignalingService : IDataPlaneSignalingService
{
    public Task<StatusResult<DataFlowResponseMessage>> StartAsync(DataflowStartMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<Void>> SuspendAsync(string dataFlowId)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<Void>> TerminateAsync(string dataFlowId, string? reason = null)
    {
        throw new NotImplementedException();
    }

    public Task<DataFlowState> GetTransferStateAsync(string processId)
    {
        throw new NotImplementedException();
    }
}