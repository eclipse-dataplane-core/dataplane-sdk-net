using Microsoft.Extensions.DependencyInjection;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddSdkServices(this IServiceCollection services, DataPlaneSdk sdk)
    {
        services.AddSingleton(sdk.Store);
        // services.AddSingleton<IDataPlaneSignalingService>(new DataPlaneSignalingService(sdk.Store, sdk));
    }
}