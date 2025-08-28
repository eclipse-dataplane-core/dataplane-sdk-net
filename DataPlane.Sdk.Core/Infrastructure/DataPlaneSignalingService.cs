using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Infrastructure;

/// <summary>
///     DataPlane Signaling (DPS) Service implementation to handle DPS requests and events.
/// </summary>
/// <param name="dataFlowContext">
///     The underlying persistence layer for <see cref="DataFlow" /> objects. Note that this
///     service acts as transaction boundary.
/// </param>
/// <param name="sdk">An instance of the <see cref="DataPlaneSdk" /> to invoke callbacks</param>
/// <param name="runtimeId">The Runtime ID of this data plane.</param>
public class DataPlaneSignalingService(DataFlowContext dataFlowContext, DataPlaneSdk sdk, string runtimeId) : IDataPlaneSignalingService
{
    public async Task<StatusResult<DataFlow>> StartAsync(DataFlowStartMessage message)
    {
        var validation = await ValidateStartMessageAsync(message);
        if (validation.IsFailed)
        {
            return StatusResult<DataFlow>.Failed(validation.Failure!);
        }

        var existingFlowResult = await dataFlowContext.FindByIdAndLeaseAsync(message.ProcessId);
        if (existingFlowResult.IsFailed)
        {
            if (existingFlowResult.Failure?.Reason == FailureReason.NotFound) // create new data flow
            {
                var dataFlow = CreateDataFlow(message);
                var sdkResult = sdk.InvokeStart(dataFlow);

                if (sdkResult.IsFailed)
                {
                    return StatusResult<DataFlow>.Failed(sdkResult.Failure!);
                }

                await dataFlowContext.UpsertAsync(dataFlow);
                await dataFlowContext.SaveChangesAsync();
                return StatusResult<DataFlow>.Success(dataFlow);
            }

            return StatusResult<DataFlow>.Failed(existingFlowResult.Failure!);
        }

        var existingFlow = existingFlowResult.Content!;
        if (existingFlow.State == DataFlowState.Started)
        {
            return StatusResult<DataFlow>.Success(existingFlow);
        }

        //update existing data flow
        var result = sdk.InvokeStart(existingFlow);
        if (result.IsFailed)
        {
            return StatusResult<DataFlow>.Failed(result.Failure!);
        }

        existingFlow = result.Content!;
        dataFlowContext.DataFlows.Update(existingFlow);
        await dataFlowContext.SaveChangesAsync();

        return StatusResult<DataFlow>.Success(existingFlow);
    }

    public async Task<StatusResult> SuspendAsync(string dataFlowId, string? reason = null)
    {
        var res = await dataFlowContext.FindByIdAndLeaseAsync(dataFlowId);
        if (res.IsFailed)
        {
            return StatusResult.Failed(res.Failure!);
        }

        var df = res.Content!;
        if (df.State == DataFlowState.Suspended) //de-duplication check
        {
            return StatusResult.Success();
        }

        var sdkResult = sdk.InvokeSuspend(df);
        if (sdkResult.IsFailed)
        {
            return StatusResult.Failed(sdkResult.Failure!);
        }

        if (df.State != DataFlowState.Started)
        {
            return StatusResult.FromCode(400, "DataFlow is not in started state, cannot suspend.");
        }

        df.Suspend(reason);
        await dataFlowContext.UpsertAsync(df);
        await dataFlowContext.SaveChangesAsync(); // commit transaction
        return sdkResult;
    }

    public async Task<StatusResult> TerminateAsync(string dataFlowId, string? reason = null)
    {
        var res = await dataFlowContext.FindByIdAndLeaseAsync(dataFlowId);
        if (res.IsFailed)
        {
            return StatusResult.Failed(res.Failure!);
        }

        var df = res.Content!;

        if (df.State == DataFlowState.Terminated) //de-duplication check
        {
            return StatusResult.Success();
        }

        var sdkResult = sdk.InvokeTerminate(df);
        if (sdkResult.IsFailed)
        {
            return StatusResult.Failed(sdkResult.Failure!);
        }

        if (df.State == DataFlowState.Provisioned)
        {
            df.Deprovision();
        }
        else
        {
            df.Terminate();
        }

        await dataFlowContext.UpsertAsync(df);
        await dataFlowContext.SaveChangesAsync(); //commit transaction
        return sdkResult;
    }

    public async Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId)
    {
        var flow = await dataFlowContext.DataFlows.FindAsync(processId);
        return flow == null ? StatusResult<DataFlowState>.NotFound() : StatusResult<DataFlowState>.Success(flow.State);
    }

    public Task<StatusResult> ValidateStartMessageAsync(DataFlowStartMessage startMessage)
    {
        // delegate validation to the SDK callback
        return Task.FromResult(sdk.InvokeValidate(startMessage));
    }

    public async Task<StatusResult<DataFlow>> PrepareAsync(DataFlowPrepareMessage prepareMessage)
    {
        var existing = await dataFlowContext.FindByIdAsync(prepareMessage.ProcessId);
        if (existing != null && existing.State != DataFlowState.Prepared && existing.State != DataFlowState.Preparing)
        {
            return StatusResult<DataFlow>.Conflict($"A data flow with ID = {existing.Id} already exists!");
        }

        var flow = existing ?? CreateDataFlow(prepareMessage);
        var result = sdk.InvokeOnPrepare(flow);

        if (result.IsFailed)
        {
            return StatusResult<DataFlow>.Failed(result.Failure!);
        }

        var updatedFlow = result.Content;
        if (updatedFlow == null)
        {
            throw new InvalidOperationException("SDK callback must return a non-null DataFlow object");
        }

        await dataFlowContext.UpsertAsync(updatedFlow);
        await dataFlowContext.SaveChangesAsync();
        return StatusResult<DataFlow>.Success(updatedFlow);
    }

    private DataFlow CreateDataFlow(DataFlowPrepareMessage message)
    {
        return new DataFlow(message.ProcessId)
        {
            Source = message.SourceDataAddress,
            Destination = message.DestinationDataAddress,
            TransferType = message.TransferType,
            RuntimeId = runtimeId,
            ParticipantId = message.ParticipantId,
            AssetId = message.DatasetId,
            AgreementId = message.AgreementId,
            CallbackAddress = message.CallbackAddress,
            Properties = message.Properties,
            State = DataFlowState.Initialized
        };
    }

    private DataFlow CreateDataFlow(DataFlowStartMessage message)
    {
        return new DataFlow(message.ProcessId)
        {
            Source = message.SourceDataAddress,
            Destination = message.DestinationDataAddress,
            TransferType = message.TransferType,
            RuntimeId = runtimeId,
            ParticipantId = message.ParticipantId,
            AssetId = message.DatasetId,
            AgreementId = message.AgreementId,
            CallbackAddress = message.CallbackAddress,
            State = DataFlowState.Initialized
        };
    }
}