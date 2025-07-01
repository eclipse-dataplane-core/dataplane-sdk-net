using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core;

public class DataPlaneSdk
{
    //todo: make the lease id configurable
    public IDataPlaneStore Store { get; set; } = new DataPlaneStatefulEntityStore(Guid.NewGuid().ToString());

    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnStart;
    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnProvision;
    public event Func<DataFlow, StatusResult<Void>>? OnTerminate;
    public event Func<DataFlow, StatusResult<Void>>? OnSuspend;
    public event Func<DataFlow, StatusResult<Void>>? OnRecover;


    public static SdkBuilder Builder()
    {
        return new SdkBuilder();
    }

    internal StatusResult<Void> InvokeTerminate(DataFlow df)
    {
        return OnTerminate != null ? OnTerminate(df) : StatusResult<Void>.Success(default);
    }

    public class SdkBuilder
    {
        private readonly DataPlaneSdk _dataPlaneSdk = new()
        {
            OnStart = _ => StatusResult<DataFlowResponseMessage>.Success(null)
        };

        public SdkBuilder Store(DataPlaneStatefulEntityStore dataPlaneStatefulEntityStore)
        {
            _dataPlaneSdk.Store = dataPlaneStatefulEntityStore;
            return this;
        }

        public DataPlaneSdk Build()
        {
            return _dataPlaneSdk;
        }

        public SdkBuilder OnStart(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnStart += processor;
            return this;
        }

        public SdkBuilder OnProvision(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnProvision += processor;
            return this;
        }

        public SdkBuilder OnTerminate(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnTerminate += processor;
            return this;
        }

        public SdkBuilder OnSuspend(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnSuspend += processor;
            return this;
        }

        public SdkBuilder OnRecover(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnRecover += processor;
            return this;
        }
    }
}