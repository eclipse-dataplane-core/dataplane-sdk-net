using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Infrastructure;

public class DataPlaneSignalingService(IDataPlaneStore dataPlaneStore, DataPlaneSdk sdk) : IDataPlaneSignalingService
{
    public Task<StatusResult<DataFlowResponseMessage>> StartAsync(DataflowStartMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<Void>> SuspendAsync(string dataFlowId)
    {
        throw new NotImplementedException();
    }

    public async Task<StatusResult<Void>> TerminateAsync(string dataFlowId, string? reason = null)
    {
        var df = await dataPlaneStore.FindByIdAsync(dataFlowId);
        //todo: terminate
        
       return sdk.InvokeTerminate(df!);
    }

    public async Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId)
    {
        var flow = await dataPlaneStore.FindByIdAsync(processId);
        return flow == null ? StatusResult<DataFlowState>.NotFound() : StatusResult<DataFlowState>.Success((DataFlowState)flow.State);
    }
}