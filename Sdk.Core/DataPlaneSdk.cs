using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public partial class DataPlaneSdk
{
    
    public IDataPlaneStore Store { get; set; } = new DataPlaneStore();
    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnStart;
    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnProvision;
    public event Func<DataFlow, StatusResult<Domain.Void>>? OnTerminate;
    public event Func<DataFlow, StatusResult<Domain.Void>>? OnSuspend;
    public event Func<DataFlow, StatusResult<Domain.Void>>? OnRecover;

    
    public static SdkBuilder Builder() => new();

    public class SdkBuilder
    {
        private readonly DataPlaneSdk _dataPlaneSdk = new()
        {
            OnStart = _ => StatusResult<DataFlowResponseMessage>.Success(null)
        };

        public SdkBuilder Store(DataPlaneStore dataPlaneStore)
        {
            _dataPlaneSdk.Store = dataPlaneStore;
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

        public SdkBuilder OnTerminate(Func<DataFlow, StatusResult<Domain.Void>> processor)
        {
            _dataPlaneSdk.OnTerminate += processor;
            return this;
        }

        public SdkBuilder OnSuspend(Func<DataFlow, StatusResult<Domain.Void>> processor)
        {
            _dataPlaneSdk.OnSuspend += processor;
            return this;
        }

        public SdkBuilder OnRecover(Func<DataFlow, StatusResult<Domain.Void>> processor)
        {
            _dataPlaneSdk.OnRecover += processor;
            return this;
        }
    }
}