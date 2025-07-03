using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;
using static Sdk.Core.Domain.IConstants;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddSdkServices(this IServiceCollection services, DataPlaneSdk sdk)
    {
        // configure HTTP Client for outgoing requests, both Control API and Data Plane Signaling
        services.AddSingleton(sdk.TokenProvider);
        services.AddTransient<AuthHeaderHandler>();
        services.AddHttpClient(HttpClientName)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddSingleton<IDataPlaneStore>(sdk.DataFlowStore);
        services.AddSingleton<IDataPlaneSignalingService>(new DataPlaneSignalingService(sdk.DataFlowStore, sdk, sdk.RuntimeId));
        services.AddTransient<IControlApiService, ControlApiService>();
    }
}