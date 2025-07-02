using Sdk.Api;
using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        var sdk = new DataPlaneSdk
        {
            Store = DataFlowContextFactory.CreatePostgres(configuration, "test-lock-id")
        };


        sdk.OnStart += _ => StatusResult<DataFlowResponseMessage>.Success(null);
        sdk.OnRecover += _ => StatusResult<Void>.Success(default);
        sdk.OnTerminate += _ => StatusResult<Void>.Success(default);
        sdk.OnSuspend += _ => StatusResult<Void>.Success(default);
        sdk.OnProvision += _ => StatusResult<DataFlowResponseMessage>.Success(null);

        // add SDK core services
        services.AddSdkServices(sdk);

        // wire up ASP.net authentication services
        services.AddSdkAuthentication(configuration);

        // wire up ASP.net authorization handlers
        services.AddSdkAuthorization();
    }
}