using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddSdkServices(this IServiceCollection services, DataPlaneSdk sdk)
    {
        services.AddSingleton<IDataPlaneStore>(sdk.DataFlowStore);
        services.AddSingleton<IDataPlaneSignalingService>(new DataPlaneSignalingService(sdk.DataFlowStore, sdk, sdk.RuntimeId));
    }
}