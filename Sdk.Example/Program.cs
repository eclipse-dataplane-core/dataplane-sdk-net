using Sdk.Core;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

// using the SDK directly
var sdk = new DataPlaneSdk
{
    Store = new DataPlaneStore()
};
sdk.OnStart += flow => StatusResult<DataFlowResponseMessage>.Success(null);
sdk.OnRecover += flow => StatusResult<Void>.Success(default);
sdk.OnTerminate += flow => StatusResult<Void>.Success(default);
sdk.OnSuspend += flow => StatusResult<Void>.Success(default);
sdk.OnProvision += flow => StatusResult<DataFlowResponseMessage>.Success(null);


// using the SDK builder
var sdk2 = DataPlaneSdk.Builder()
    .Store(new DataPlaneStore())
    .OnStart(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnProvision(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnSuspend(flow => StatusResult<Void>.Success(default))
    .OnTerminate(flow => StatusResult<Void>.Success(default))
    .OnRecover(flow => StatusResult<Void>.Success(default))
    .Build();