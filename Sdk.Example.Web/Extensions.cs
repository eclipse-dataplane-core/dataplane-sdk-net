using Microsoft.IdentityModel.Tokens;
using Sdk.Api;
using Sdk.Core;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using static Sdk.Core.Data.DataFlowContextFactory;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        const string exampleRuntimeId = "example-runtime-id";
        var sdk = new DataPlaneSdk
        {
            DataFlowStore = CreatePostgres(configuration, exampleRuntimeId),
            RuntimeId = exampleRuntimeId,
            OnStart = _ => StatusResult<DataFlowResponseMessage>.Success(null),
            OnRecover = _ => StatusResult<Void>.Success(default),
            OnTerminate = _ => StatusResult<Void>.Success(default),
            OnSuspend = _ => StatusResult<Void>.Success(default),
            OnProvision = _ => StatusResult<DataFlowResponseMessage>.Success(null)
        };

        // read required configuration from appsettings.json
        services.Configure<ControlApiOptions>(configuration.GetSection("ControlApi"));

        // add SDK core services
        services.AddSdkServices(sdk);

        // wire up ASP.net authentication services
        services.AddSdkAuthentication(configuration);

        // overwrite SDK authentication with KeycloakJWT. Effectively, this sets the default authentication scheme to "KeycloakJWT",
        // foregoing the SDK default authentication scheme ("DataPlaneSdkJWT").
        services.AddAuthentication("KeycloakJWT")
            .AddJwtBearer("KeycloakJWT", options =>
            {
                // Configure Keycloak as the Identity Provider
                options.Authority = "http://localhost:8080/realms/master";
                options.RequireHttpsMetadata = false; // Only for develop

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "http://localhost:8080/realms/master",
                    ValidateAudience = true,
                    ValidAudience = "dataplane-api",
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateActor = false,
                    ValidateTokenReplay = true
                };
            });

        // wire up ASP.net authorization handlers
        services.AddSdkAuthorization();
    }
}