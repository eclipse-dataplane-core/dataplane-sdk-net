using Sdk.Core.Data;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core;

public class DataPlaneSdk
{
    public Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnProvision;
    public Func<DataFlow, StatusResult<Void>>? OnRecover;
    public Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnStart;
    public Func<DataFlow, StatusResult<Void>>? OnSuspend;
    public Func<DataFlow, StatusResult<Void>>? OnTerminate;
    public Func<DataflowStartMessage, StatusResult<Void>>? OnValidateStartMessage;

    //todo: make the lease id configurable
    public DataFlowContext DataFlowStore { get; set; } = DataFlowContextFactory.CreateInMem("test-lock-id");
    public string RuntimeId { get; set; } = Guid.NewGuid().ToString();
    public ITokenProvider TokenProvider { get; set; } = new NoopTokenProvider();

    public static SdkBuilder Builder()
    {
        return new SdkBuilder();
    }

    internal StatusResult<Void> InvokeTerminate(DataFlow df)
    {
        return OnTerminate != null ? OnTerminate(df) : StatusResult<Void>.Success(default);
    }

    internal StatusResult<Void> InvokeSuspend(DataFlow df)
    {
        return OnSuspend != null ? OnSuspend(df) : StatusResult<Void>.Success(default);
    }

    internal StatusResult<DataFlowResponseMessage> InvokeStart(DataFlow df)
    {
        return OnStart != null
            ? OnStart(df)
            : StatusResult<DataFlowResponseMessage>.Success(new DataFlowResponseMessage
            {
                DataAddress = df.Destination
            });
    }

    internal StatusResult<Void> InvokeValidate(DataflowStartMessage startMessage)
    {
        return OnValidateStartMessage?.Invoke(startMessage) ?? StatusResult<Void>.Success(default);
    }

    public class SdkBuilder
    {
        private readonly DataPlaneSdk _dataPlaneSdk = new()
        {
            OnStart = _ => StatusResult<DataFlowResponseMessage>.Success(null)
        };

        public SdkBuilder Store(DataFlowContext dataPlaneStatefulEntityStore)
        {
            _dataPlaneSdk.DataFlowStore = dataPlaneStatefulEntityStore;
            return this;
        }

        public DataPlaneSdk Build()
        {
            return _dataPlaneSdk;
        }

        public SdkBuilder OnStart(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnStart = processor;
            return this;
        }

        public SdkBuilder OnProvision(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnProvision = processor;
            return this;
        }

        public SdkBuilder OnTerminate(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnTerminate = processor;
            return this;
        }

        public SdkBuilder OnSuspend(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnSuspend = processor;
            return this;
        }

        public SdkBuilder OnRecover(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnRecover = processor;
            return this;
        }

        public SdkBuilder OnValidateStartMessage(Func<DataflowStartMessage, StatusResult<Void>> processor)
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