using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Authorization;

namespace Sdk.Api;

public static class SdkApiExtension
{
    public static void AddSdkAuthorizationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, DataFlowAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, FooAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy("DataFlowAccess", policy =>
                policy.Requirements.Add(new DataFlowRequirement()))
            .AddPolicy("FooAccess", policy =>
                policy.Requirements.Add(new FooRequirement()));
    }
}