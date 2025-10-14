using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DataPlane.Sdk.Core;

public class DataPlaneSdk
{
    public Func<DataFlow, StatusResult>? OnComplete;
    public Func<DataFlow, StatusResult<DataFlow>>? OnPrepare;
    public Func<DataFlow, StatusResult<DataFlow>>? OnStart;
    public Func<DataFlow, StatusResult>? OnSuspend;
    public Func<DataFlow, StatusResult>? OnTerminate;

    //todo: make the lease id configurable
    public Func<IDataPlaneStore> DataFlowStore { get; set; } = () => DataFlowContextFactory.CreateInMem("test-lock-id");
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
        df.State = DataFlowState.Started;
        return OnStart != null ? OnStart(df) : StatusResult<DataFlow>.Success(df);
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

    internal StatusResult InvokeOnComplete(DataFlow flow)
    {
        return OnComplete != null ? OnComplete(flow) : StatusResult.Success();
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

        public SdkBuilder OnComplete(Func<DataFlow, StatusResult> processor)
        {
            _dataPlaneSdk.OnComplete = processor;
            return this;
        }

        public SdkBuilder RuntimeId(string runtimeId)
        {
            _dataPlaneSdk.RuntimeId = runtimeId;
            return this;
        }
    }
}