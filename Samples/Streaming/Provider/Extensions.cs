using DataPlane.Sdk.Api;
using DataPlane.Sdk.Core;
using DataPlane.Sdk.Core.Domain.Model;
using Microsoft.IdentityModel.Tokens;
using Provider.Nats;
using Provider.Services;
using static DataPlane.Sdk.Core.Data.DataFlowContextFactory;

namespace Provider;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        var dataplaneConfig = configuration.GetSection("DataPlaneSdk");
        var config = dataplaneConfig.Get<DataPlaneSdkOptions>() ?? throw new ArgumentException("Configuration invalid!");
        var dataFlowContext = () => CreatePostgres(configuration, config.RuntimeId);


        var sdk = new DataPlaneSdk
        {
            DataFlowStore = dataFlowContext,
            RuntimeId = config.RuntimeId,
            OnStart = dataFlow =>
            {
                var dataService = services.BuildServiceProvider().GetRequiredService<IDataService>();
                return dataService.ProcessStart(dataFlow);
            },
            OnTerminate = df =>
            {
                var dataService = services.BuildServiceProvider().GetRequiredService<IDataService>();
                var task = dataService.ProcessTerminate(df);
                task.Wait();
                return task.Result;
            },
            OnSuspend = _ => StatusResult.Success(),
            OnPrepare = _ => throw new NotImplementedException("Cannot call /prepare on a provider data plane"),
            OnComplete = _ => StatusResult.Success()
        };

        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<INatsPublisherService, NatsPublisherService>();


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