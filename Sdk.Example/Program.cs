using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

// using the SDK directly
var sdk = new DataPlaneSdk
{
    Store = DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString())
};
sdk.OnStart += flow => StatusResult<DataFlowResponseMessage>.Success(null);
sdk.OnRecover += flow => StatusResult<Void>.Success(default);
sdk.OnTerminate += flow => StatusResult<Void>.Success(default);
sdk.OnSuspend += flow => StatusResult<Void>.Success(default);
sdk.OnProvision += flow => StatusResult<DataFlowResponseMessage>.Success(null);


// using the SDK builder
var sdk2 = DataPlaneSdk.Builder()
    .Store(DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString()))
    .OnStart(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnProvision(flow => StatusResult<DataFlowResponseMessage>.Success(null))
    .OnSuspend(flow => StatusResult<Void>.Success(default))
    .OnTerminate(flow => StatusResult<Void>.Success(default))
    .OnRecover(flow => StatusResult<Void>.Success(default))
    .Build();