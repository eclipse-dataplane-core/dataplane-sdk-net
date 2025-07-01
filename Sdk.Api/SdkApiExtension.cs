using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sdk.Core.Authorization.DataFlows;
using Sdk.Core.Authorization.Foo;
using Sdk.Core.Domain;

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
        services.AddSingleton<IAuthorizationHandler, FooAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy("DataFlowAccess", policy =>
                policy.Requirements.Add(new DataFlowRequirement()))
            .AddPolicy("FooAccess", policy =>
                policy.Requirements.Add(new FooRequirement()));
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
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Disable built-in validation via fake parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = configuration.GetValue<bool>("Token:ValidateIssuer"),
                    ValidateAudience = configuration.GetValue<bool>("Token:ValidateAudience"),
                    ValidateIssuerSigningKey = configuration.GetValue<bool>("Token:ValidateIssuerSigningKey"),
                    ValidateLifetime = configuration.GetValue<bool>("Token:ValidateLifetime"),
                    ValidateActor = false,
                    ValidateTokenReplay = configuration.GetValue<bool>("Token:ValidateTokenReplay"),
                    SignatureValidator = (token, tp) => new JsonWebTokenHandler().ReadJsonWebToken(token)
                };
                // Custom logic example: additional validation
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = _ => Task.CompletedTask,
                    OnAuthenticationFailed = c => Task.FromException(c.Exception),
                    OnMessageReceived = _ => Task.CompletedTask
                };
            });
    }
}