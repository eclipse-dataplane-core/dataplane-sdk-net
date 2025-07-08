using DataPlane.Sdk.Api;
using DataPlane.Sdk.Core;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using DataPlane.Sdk.Core.Infrastructure;
using static DataPlane.Sdk.Core.Data.DataFlowContextFactory;
using Void = DataPlane.Sdk.Core.Domain.Void;

namespace DataPlane.Sdk.Example.Web;

public static class Extensions
{
    public static void AddDataPlaneSdk(this IServiceCollection services, IConfiguration configuration)
    {
        // initialize and configure the DataPlaneSdk
        var config = configuration.GetSection("DataPlaneSdk").Get<DataPlaneSdkOptions>() ?? throw new ArgumentException("Configuration invalid!");
        var sdk = new DataPlaneSdk
        {
            DataFlowStore = CreateInMem("example-leaser"),
            RuntimeId = config.RuntimeId,
            OnStart = f => StatusResult<DataFlowResponseMessage>.Success(new DataFlowResponseMessage { DataAddress = f.Destination }),
            OnRecover = _ => StatusResult<Void>.Success(default),
            OnTerminate = _ => StatusResult<Void>.Success(default),
            OnSuspend = _ => StatusResult<Void>.Success(default),
            OnProvision = f => StatusResult<DataFlowResponseMessage>.Success(new DataFlowResponseMessage { DataAddress = f.Destination })
        };

        // read required configuration from appsettings.json to make it injectable
        services.Configure<ControlApiOptions>(configuration.GetSection("DataPlaneSdk:ControlApi"));

        // add SDK core services
        services.AddSdkServices(sdk);

        // wire up ASP.net authentication services
        services.AddSdkAuthentication(configuration);

        // overwrite SDK authentication with KeycloakJWT. Effectively, this sets the default authentication scheme to "KeycloakJWT",
        // foregoing the SDK default authentication scheme ("DataPlaneSdkJWT").
        // Note that this assumes that KeyCloak is up-and-running. 

        // services.AddAuthentication("KeycloakJWT")
        //     .AddJwtBearer("KeycloakJWT", options =>
        //     {
        //         // Configure Keycloak as the Identity Provider
        //         options.Authority = "http://localhost:8080/realms/master";
        //         options.RequireHttpsMetadata = false; // Only for develop
        //
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidIssuer = "http://localhost:8080/realms/master",
        //             ValidateAudience = true,
        //             ValidAudience = "dataplane-api",
        //             ValidateIssuerSigningKey = true,
        //             ValidateLifetime = true,
        //             ValidateActor = false,
        //             ValidateTokenReplay = true
        //         };
        //     });

        // wire up ASP.net authorization handlers
        services.AddSdkAuthorization();
    }
}