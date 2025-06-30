using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddSdkCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDataPlaneSignalingService, DataPlaneSignalingService>();
    }

    public static void AddDefaultStorage(this IServiceCollection services)
    {
        services.AddSingleton<IDataPlaneStore, DataPlaneStore>();
    }
}