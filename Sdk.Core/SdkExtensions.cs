using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Core.Authorization;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public static class SdkExtensions
{
    public static void AddFromSdk(this IServiceCollection services, DataPlaneSdk sdk)
    {
        services.AddSingleton(sdk.Store);
        services.AddSingleton<IDataPlaneSignalingService>(new DataPlaneSignalingService(sdk.Store, sdk));
        services.AddSingleton<IApiAuthorizationService>(new AuthServiceDelegate(sdk.AuthorizationHandler));
    }
}

/// <summary>
/// wraps the SDK's authorization handler to implement the IApiAuthorizationService interface.
/// </summary>
internal class AuthServiceDelegate(Func<string, ClaimsPrincipal, Task<bool>> sdkAuthorizationHandler)
    : IApiAuthorizationService
{
    public Task<bool> AuthorizeAsync(string participantContext, ClaimsPrincipal claimsPrincipal)
    {
        return sdkAuthorizationHandler.Invoke(participantContext, claimsPrincipal);
    }
}