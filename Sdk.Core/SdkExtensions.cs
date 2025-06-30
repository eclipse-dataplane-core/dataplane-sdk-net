using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Authorization;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddSdkServices(this IServiceCollection services, DataPlaneSdk sdk)
    {
        services.AddSingleton(sdk.Store);
        services.AddSingleton<IDataPlaneSignalingService>(new DataPlaneSignalingService(sdk.Store, sdk));
        


    }
}