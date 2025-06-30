using Sdk.Core;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Extension;

namespace Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services)
    {
        var sdk = new DataPlaneSdk
        {
            Store = new PostgresDataPlaneStore(),
        };
        sdk.OnStart += _ => StatusResult<DataFlowResponseMessage>.Success(null);
        sdk.OnRecover += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnTerminate += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnSuspend += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnProvision += _ => StatusResult<DataFlowResponseMessage>.Success(null);

        services.AddFromSdk(sdk);
    }
}