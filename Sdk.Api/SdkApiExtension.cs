using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sdk.Api.Authorization.DataFlows;
using Sdk.Core.Domain.Model;

namespace Sdk.Api;

public static class SdkApiExtension
{
    /// <summary>
    ///     Adds authorization handlers for every type of resource, such as <see cref="DataFlow" />
    /// </summary>
    /// <param name="services"></param>
    public static void AddSdkAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, DataFlowAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy("DataFlowAccess", policy =>
                policy.Requirements.Add(new DataFlowRequirement()));
    }

    /// <summary>
    ///     Registers a basic authentication handler for the SDK. By default, it uses JWT Bearer authentication, validates
    ///     the following token claims:
    ///     <list type="bullet">
    ///         <item>token lifetime (not expired, already valid)</item>
    ///         <item>audience: configured via <c>Token:ValidateAudience</c> in appsettings.json</item>
    ///         <item>issuer: configured via <c>Token:ValidateIssuer</c> in appsettings.json</item>
    ///     </list>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddSdkAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // add authentication handler
        services.AddAuthentication("DataPlaneSdkJWT")
            .AddJwtBearer("DataPlaneSdkJWT", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration.GetValue<string>("Token:ValidIssuer"),
                    ValidateAudience = true,
                    ValidAudience = configuration.GetValue<string>("Token:ValidAudience"),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateActor = false,
                    ValidateTokenReplay = true
                };
            });
    }
}