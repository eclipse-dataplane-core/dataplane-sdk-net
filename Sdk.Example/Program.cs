using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sdk.Core;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using static Sdk.Core.Data.DataFlowContextFactory;
using Void = Sdk.Core.Domain.Void;

// using the SDK directly

namespace Sdk.Example;

internal class Program
{
    private static async Task Main(string[] args)
    {
        DataPlaneSdk sdk;
        var config = new DataPlaneSdkOptions();
        // instantiate SDK, register callbacks etc.
        // set up IHost for dependency injection
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((c, services) =>
            {
                // read required configuration from appsettings.json
                c.Configuration.GetSection("DataPlaneSdk").Bind(config);
                services.Configure<ControlApiOptions>(c.Configuration.GetSection("ControlApi"));
                services.Configure<DataPlaneSdkOptions>(c.Configuration.GetSection("DataPlaneSdk"));
                // use the SDK's extension method to register all services provided by the SDK
                sdk = CreateSdk(c.Configuration.GetConnectionString("DefaultConnection"), config);
                services.AddSdkServices(sdk);
            })
            .Build();

        var logger = host.Services.GetService<ILogger<Program>>()!;
        logger.LogInformation("DataPlane SDK Configuration: {Serialize}", JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));


        // register the data plane instance with the control plane
        var controlService = host.Services.GetRequiredService<IControlApiService>();
        logger.LogInformation("Registering data plane with control plane");
        await controlService.RegisterDataPlane(new DataPlaneInstance(config.InstanceId)
        {
            Url = config.PublicUrl,
            State = DataPlaneState.Available,
            AllowedSourceTypes = config.AllowedSourceTypes,
            AllowedTransferTypes = config.AllowedTransferTypes
        });
    }

    private static DataPlaneSdk CreateSdk(string? connectionString, DataPlaneSdkOptions options)
    {
        var sdk = new DataPlaneSdk
        {
            RuntimeId = options.RuntimeId,
            DataFlowStore = connectionString != null ? CreatePostgres(connectionString, options.RuntimeId) : CreateInMem(options.RuntimeId),
            OnStart = flow => StatusResult<DataFlowResponseMessage>.Success(null),
            OnRecover = flow => StatusResult<Void>.Success(default),
            OnTerminate = flow => StatusResult<Void>.Success(default),
            OnSuspend = flow => StatusResult<Void>.Success(default),
            OnProvision = flow => StatusResult<DataFlowResponseMessage>.Success(null),
            OnValidateStartMessage = msg => StatusResult<Void>.Success(default)
        };

        // alternatively using the SDK builder
        var sdk2 = DataPlaneSdk.Builder()
            .RuntimeId(options.RuntimeId)
            .Store(CreateInMem(Guid.NewGuid().ToString()))
            .OnStart(flow => StatusResult<DataFlowResponseMessage>.Success(null))
            .OnProvision(flow => StatusResult<DataFlowResponseMessage>.Success(null))
            .OnSuspend(flow => StatusResult<Void>.Success(default))
            .OnTerminate(flow => StatusResult<Void>.Success(default))
            .OnRecover(flow => StatusResult<Void>.Success(default))
            .OnValidateStartMessage(msg => StatusResult<Void>.Success(default))
            .Build();
        return sdk;
    }
}