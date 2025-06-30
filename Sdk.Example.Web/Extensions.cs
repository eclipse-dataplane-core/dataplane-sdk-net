using Microsoft.AspNetCore.Authorization;
using Sdk.Api;
using Sdk.Core;
using Sdk.Core.Authorization;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Postgres;

namespace Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        var sdk = new DataPlaneSdk
        {
            Store = new PostgresDataPlaneStore(new DataFlowContextFactory(configuration)),
            // plug in custom authorization middleware:
            // AuthorizationHandler = (participantContextId, claimsPrincipal) =>
            // {
            //     // todo: implement custom authorization logic
            //     return Task.FromResult(true);
            // }
        };

        sdk.OnStart += _ => StatusResult<DataFlowResponseMessage>.Success(null);
        sdk.OnRecover += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnTerminate += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnSuspend += _ => StatusResult<Core.Domain.Void>.Success(default);
        sdk.OnProvision += _ => StatusResult<DataFlowResponseMessage>.Success(null);

        sdk.OnAuthentication += token => Task.CompletedTask;

        // add SDK core services
        services.AddSdkServices(sdk);
        
        // wire up authorization handlers
        services.AddSdkAuthorizationServices();
    }
}

