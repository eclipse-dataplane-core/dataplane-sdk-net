using DataPlane.Sdk.Api;
using DataPlane.Sdk.Core;
using DataPlane.Sdk.Core.Domain.Model;
using HttpDataplane.Services;
using Microsoft.IdentityModel.Tokens;
using static DataPlane.Sdk.Core.Data.DataFlowContextFactory;

namespace HttpDataplane;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        var dataplaneConfig = configuration.GetSection("DataPlaneSdk");
        var config = dataplaneConfig.Get<DataPlaneSdkOptions>() ?? throw new ArgumentException("Configuration invalid!");
        var permissionService = new DataService();
        var sdk = new DataPlaneSdk
        {
            DataFlowStore = () =>
                CreatePostgres(configuration, config.RuntimeId),
            RuntimeId = config.RuntimeId,
            OnStart = f => StatusResult<DataFlow>.Success(permissionService.SetPublicEndpoint(f)),
            OnRecover = _ => StatusResult.Success(),
            OnTerminate = _ => StatusResult.Success(),
            OnSuspend = _ => StatusResult.Success(),
            OnPrepare = f =>
            {
                f.State = DataFlowState.Prepared;
                return StatusResult<DataFlow>.Success(f);
            }
        };

        services.AddSingleton<IDataService>(permissionService);
        services.AddSingleton<IHttpProxyService, HttpProxyService>();


        // add SDK core services
        services.AddSdkServices(sdk, dataplaneConfig);

        // Configuration for keycloak. Effectively, this sets the default authentication scheme to "KeycloakJWT",
        // foregoing the SDK default authentication scheme and using Keycloak as the identity provider.
        // This assumes that Keycloak is running on http://keycloak:8080, which is the default if launched with docker-compose.

        var jwtSettings = configuration.GetSection("JwtSettings");
        services.AddAuthentication("KeycloakJWT")
            .AddJwtBearer("KeycloakJWT", options =>
            {
                // Configure Keycloak as the Identity Provider
                options.Authority = jwtSettings["Authority"];
                options.RequireHttpsMetadata = false; // Only for develop

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
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