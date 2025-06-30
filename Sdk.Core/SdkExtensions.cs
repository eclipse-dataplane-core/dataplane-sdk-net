using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddFromSdk(this IServiceCollection services, DataPlaneSdk sdk)
    {
        services.AddSingleton(sdk.Store);
    }
}