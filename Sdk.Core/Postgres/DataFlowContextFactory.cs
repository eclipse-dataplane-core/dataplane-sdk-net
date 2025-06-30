using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sdk.Core.Postgres;

public class DataFlowContextFactory(IConfiguration configuration)
{
    public async Task<DataFlowContext> Create()
    {
        var options = new DbContextOptionsBuilder<DataFlowContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;


        var dataFlowContext = new DataFlowContext(options);

        if (bool.TryParse(configuration.GetSection("Database:AutoMigrate").Value, out var result) && result)
        {
            await dataFlowContext.Database.EnsureCreatedAsync();
        }


        return dataFlowContext;
    }
}