using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using Void = Sdk.Core.Domain.Void;

// using the SDK directly
var sdk = new DataPlaneSdk
{
    DataFlowStore = DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString())
};
sdk.OnStart += flow => StatusResult<DataFlowResponseMessage>.Success(null);
sdk.OnRecover += flow => StatusResult<Void>.Success(default);
sdk.OnTerminate += flow => StatusResult<Void>.Success(default);
sdk.OnSuspend += flow => StatusResult<Void>.Success(default);
sdk.OnProvision += flow => StatusResult<DataFlowResponseMessage>.Success(null);
sdk.OnValidateStartMessage += msg => StatusResult<Void>.Success(default);


// using the SDK builder
var sdk2 = DataPlaneSdk.Builder()
    .Store(DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString()))
    .OnStart(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnProvision(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnSuspend(flow => StatusResult<Void>.Success(default))
    .OnTerminate(flow => StatusResult<Void>.Success(default))
    .OnRecover(flow => StatusResult<Void>.Success(default))
    .OnValidateStartMessage(msg => StatusResult<Void>.Success(default))
    .Build();