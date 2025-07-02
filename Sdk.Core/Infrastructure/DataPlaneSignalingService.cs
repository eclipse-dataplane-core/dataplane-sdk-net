using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core.Infrastructure;

public class DataPlaneSignalingService(DataFlowContext dataFlowContext, DataPlaneSdk sdk) : IDataPlaneSignalingService
{
    public Task<StatusResult<DataFlowResponseMessage>> StartAsync(DataflowStartMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<Void>> SuspendAsync(string dataFlowId, string? reason)
    {
        throw new NotImplementedException();
    }

    public async Task<StatusResult<Void>> TerminateAsync(string dataFlowId, string? reason = null)
    {
        var df = await dataFlowContext.FindByIdAsync(dataFlowId);
        if (df == null || df.State == (int)DataFlowState.Terminated)
        {
            return StatusResult<Void>.NotFound();
        }

        if (df.State == (int)DataFlowState.Provisioned)
        {
            df.Deprovision();
        }
        else
        {
            df.Terminate();
        }

        await dataFlowContext.SaveAsync(df);


        var res = sdk.InvokeTerminate(df);
        await dataFlowContext.SaveChangesAsync(); //commit transaction
        return res;
    }

    public async Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId)
    {
        var flow = await dataFlowContext.DataFlows.FindAsync(processId);
        return flow == null ? StatusResult<DataFlowState>.NotFound() : StatusResult<DataFlowState>.Success((DataFlowState)flow.State);
    }
}