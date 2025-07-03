using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

// using the SDK directly

namespace Sdk.Example;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // instantiate SDK, register callbacks etc.
        var sdk = CreateSdk();
        // set up IHost for dependency injection
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((c, services) =>
            {
                // read required configuration from appsettings.json
                services.Configure<ControlApiOptions>(c.Configuration.GetSection("ControlApi"));
                // use the SDK's extension method to register all services provided by the SDK
                services.AddSdkServices(sdk);
            })
            .Build();

        // register the data plane instance with the control plane
        var controlService = host.Services.GetRequiredService<IControlApiService>();
        await controlService.RegisterDataPlane(new DataPlaneInstance("example-dataplane-id")
        {
            Url = new Uri("http://localhost/dataplane"),
            State = DataPlaneState.Available,
            AllowedSourceTypes = ["example-source-type"],
            AllowedTransferTypes = ["example-transfer-type"]
        });
    }

    private static DataPlaneSdk CreateSdk()
    {
        var sdk = new DataPlaneSdk
        {
            RuntimeId = "example-runtime-id",
            DataFlowStore = DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString()),
            OnStart = flow => StatusResult<DataFlowResponseMessage>.Success(null),
            OnRecover = flow => StatusResult<Void>.Success(default),
            OnTerminate = flow => StatusResult<Void>.Success(default),
            OnSuspend = flow => StatusResult<Void>.Success(default),
            OnProvision = flow => StatusResult<DataFlowResponseMessage>.Success(null),
            OnValidateStartMessage = msg => StatusResult<Void>.Success(default)
        };

        // alternatively using the SDK builder
        var sdk2 = DataPlaneSdk.Builder()
            .RuntimeId("example-runtime-id")
            .Store(DataFlowContextFactory.CreateInMem(Guid.NewGuid().ToString()))
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