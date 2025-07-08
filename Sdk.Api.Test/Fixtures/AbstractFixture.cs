using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Core;
using Sdk.Core.Data;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Api.Test.Fixtures;

public class AbstractFixture : IDisposable
{
    private WebApplicationFactory<Program>? _factory;
    public HttpClient Client { get; private set; }
    public DataFlowContext Context { get; protected init; }
    public DataPlaneSdk Sdk { get; } = new();

    public void Dispose()
    {
        _factory?.Dispose();
        Client.Dispose();
        Context.Dispose();
    }

    internal void InitializeFixture(DataFlowContext context, IDataPlaneSignalingService service)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDataPlaneStore>(context);
                services.AddSingleton(service);
            });
        });
        Client = _factory.CreateClient();
    }
}