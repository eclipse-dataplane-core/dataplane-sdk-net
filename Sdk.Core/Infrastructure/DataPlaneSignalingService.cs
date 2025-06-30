using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Infrastructure;

public class DataPlaneSignalingService : IDataPlaneSignalingService
{
    private readonly IDataPlaneStore _dataPlaneStore;

    public DataPlaneSignalingService(IDataPlaneStore dataPlaneStore)
    {
        _dataPlaneStore = dataPlaneStore;
    }

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

    public async Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId)
    {
        var flow = await _dataPlaneStore.FindByIdAsync(processId);
        return flow == null ? StatusResult<DataFlowState>.NotFound() : StatusResult<DataFlowState>.Success((DataFlowState)flow.State);
    }
}