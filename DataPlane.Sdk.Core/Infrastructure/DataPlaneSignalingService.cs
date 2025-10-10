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
public class DataPlaneSignalingService(IDataPlaneStore dataFlowContext, DataPlaneSdk sdk) : IDataPlaneSignalingService
{
    private readonly string _runtimeId = sdk.RuntimeId;

    public async Task<StatusResult<DataFlow>> StartAsync(DataFlowStartMessage message)
    {
        var existingFlowResult = await dataFlowContext.FindByIdAsync(message.ProcessId);

        if (existingFlowResult != null)
        {
            return StatusResult<DataFlow>.Conflict("A data flow with ID = " + message.ProcessId + " already exists!");
        }

        var dataFlow = CreateDataFlow(message);

        return await StartExistingFlow(dataFlow, sdk.InvokeStart);
    }


    public async Task<StatusResult<DataFlow>> StartByIdAsync(string id, DataFlowStartByIdMessage message)
    {
        var existing = await dataFlowContext.FindByIdAsync(id);

        if (existing == null)
        {
            return StatusResult<DataFlow>.NotFound();
        }

        if (existing.State == DataFlowState.Started) // de-duplication check
        {
            return StatusResult<DataFlow>.Success(existing);
        }

        // check the correct state of the existing DF
        if (existing.State is DataFlowState.Starting or DataFlowState.Prepared or DataFlowState.Uninitialized)
        {
            return await StartExistingFlow(existing, sdk.InvokeStart);
        }

        return StatusResult<DataFlow>.Conflict($"DataFlow in wrong state: {existing.State}, expected one of: Starting, Prepared, Uninitialized");
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
        await dataFlowContext.UpsertAsync(df, true);
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

        df.Terminate();

        await dataFlowContext.UpsertAsync(df, true);
        return sdkResult;
    }

    public async Task<StatusResult<DataFlowState>> GetTransferStateAsync(string processId)
    {
        var flow = await dataFlowContext.FindByIdAsync(processId);
        return flow == null ? StatusResult<DataFlowState>.NotFound() : StatusResult<DataFlowState>.Success(flow.State);
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

        await dataFlowContext.UpsertAsync(updatedFlow, true);
        return StatusResult<DataFlow>.Success(updatedFlow);
    }

    private async Task<StatusResult<DataFlow>> StartExistingFlow(DataFlow existingFlow, Func<DataFlow, StatusResult<DataFlow>> sdkHandler)
    {
        // invoke SDK handler
        var sdkResult = sdkHandler(existingFlow);

        if (sdkResult.IsFailed)
        {
            return StatusResult<DataFlow>.Failed(sdkResult.Failure!);
        }

        // check the correct state from the SDK handler
        var state = sdkResult.Content?.State;
        if (state != DataFlowState.Started && state != DataFlowState.Starting)
        {
            return StatusResult<DataFlow>.Conflict($"Wrong state from SDK handler: {state}");
        }

        existingFlow = sdkResult.Content!;

        await dataFlowContext.UpsertAsync(existingFlow, true);
        return StatusResult<DataFlow>.Success(existingFlow);
    }

    private DataFlow CreateDataFlow(DataFlowPrepareMessage message)
    {
        return new DataFlow(message.ProcessId)
        {
            Destination = message.DestinationDataAddress,
            TransferType = message.TransferType,
            RuntimeId = _runtimeId,
            ParticipantId = message.ParticipantId,
            AssetId = message.DatasetId,
            AgreementId = message.AgreementId,
            CallbackAddress = message.CallbackAddress,
            State = DataFlowState.Uninitialized
        };
    }

    private DataFlow CreateDataFlow(DataFlowStartMessage message)
    {
        return new DataFlow(message.ProcessId)
        {
            Source = message.SourceDataAddress,
            Destination = message.DestinationDataAddress,
            TransferType = message.TransferType,
            RuntimeId = _runtimeId,
            ParticipantId = message.ParticipantId,
            AssetId = message.DatasetId,
            AgreementId = message.AgreementId,
            CallbackAddress = message.CallbackAddress,
            State = DataFlowState.Uninitialized
        };
    }
}