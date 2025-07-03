using Microsoft.IdentityModel.Tokens;
using Sdk.Api;
using Sdk.Core;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using static Sdk.Core.Data.DataFlowContextFactory;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        var sdk = new DataPlaneSdk
        {
            DataFlowStore = CreatePostgres(configuration, "test-lock-id"),
            RuntimeId = "example-runtime-id"
        };

        sdk.OnStart += _ => StatusResult<DataFlowResponseMessage>.Success(null);
        sdk.OnRecover += _ => StatusResult<Void>.Success(default);
        sdk.OnTerminate += _ => StatusResult<Void>.Success(default);
        sdk.OnSuspend += _ => StatusResult<Void>.Success(default);
        sdk.OnProvision += _ => StatusResult<DataFlowResponseMessage>.Success(null);

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