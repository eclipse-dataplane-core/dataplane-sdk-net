using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DataPlane.Sdk.Core;

public class DataPlaneSdk
{
    public Func<DataFlow, StatusResult<DataFlow>>? OnPrepare;
    public Func<DataFlow, StatusResult>? OnRecover;
    public Func<DataFlow, StatusResult<DataFlow>>? OnStart;
    public Func<DataFlow, StatusResult>? OnSuspend;
    public Func<DataFlow, StatusResult>? OnTerminate;
    public Func<DataFlowStartMessage, StatusResult>? OnValidateStartMessage;

    //todo: make the lease id configurable
    public Func<DataFlowContext> DataFlowStore { get; set; } = () => DataFlowContextFactory.CreateInMem("test-lock-id");
    public string RuntimeId { get; set; } = Guid.NewGuid().ToString();
    public ITokenProvider TokenProvider { get; set; } = new NoopTokenProvider(LoggerFactory.Create(_ => { }).CreateLogger<NoopTokenProvider>());

    public static SdkBuilder Builder()
    {
        return new SdkBuilder();
    }

    internal StatusResult InvokeTerminate(DataFlow df)
    {
        return OnTerminate != null ? OnTerminate(df) : StatusResult.Success();
    }

    internal StatusResult InvokeSuspend(DataFlow df)
    {
        return OnSuspend != null ? OnSuspend(df) : StatusResult.Success();
    }

    internal StatusResult<DataFlow> InvokeStart(DataFlow df)
    {
        if (OnStart != null)
        {
            return OnStart(df);
        }

        df.State = DataFlowState.Started;
        return StatusResult<DataFlow>.Success(df);
    }

    internal StatusResult InvokeValidate(DataFlowStartMessage startMessage)
    {
        return OnValidateStartMessage?.Invoke(startMessage) ?? StatusResult.Success();
    }

    internal StatusResult<DataFlow> InvokeOnPrepare(DataFlow flow)
    {
        if (OnPrepare != null)
        {
            return OnPrepare(flow);
        }

        flow.State = DataFlowState.Prepared;
        return StatusResult<DataFlow>.Success(flow);
    }

    public class SdkBuilder
    {
        private readonly DataPlaneSdk _dataPlaneSdk = new()
        {
            OnStart = StatusResult<DataFlow>.Success
        };

        public SdkBuilder Store(Func<DataFlowContext> dataPlaneStatefulEntityStore)
        {
            _dataPlaneSdk.DataFlowStore = dataPlaneStatefulEntityStore;
            return this;
        }

        public DataPlaneSdk Build()
        {
            return _dataPlaneSdk;
        }

        public SdkBuilder OnProvision(Func<DataFlow, StatusResult<DataFlow>> processor)
        {
            _dataPlaneSdk.OnPrepare = processor;
            return this;
        }

        public SdkBuilder OnTerminate(Func<DataFlow, StatusResult> processor)
        {
            _dataPlaneSdk.OnTerminate = processor;
            return this;
        }

        public SdkBuilder OnSuspend(Func<DataFlow, StatusResult> processor)
        {
            _dataPlaneSdk.OnSuspend = processor;
            return this;
        }

        public SdkBuilder OnRecover(Func<DataFlow, StatusResult> processor)
        {
            _dataPlaneSdk.OnRecover = processor;
            return this;
        }

        public SdkBuilder OnValidateStartMessage(Func<DataFlowStartMessage, StatusResult> processor)
        {
            _dataPlaneSdk.OnValidateStartMessage = processor;
            return this;
        }

        public SdkBuilder RuntimeId(string runtimeId)
        {
            _dataPlaneSdk.RuntimeId = runtimeId;
            return this;
        }
    }
}