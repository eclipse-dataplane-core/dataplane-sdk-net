using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sdk.Core.Data;

public class DataFlowContextFactory
{
    public static DataFlowContext CreatePostgres(string connectionString, string lockId, bool autoMigrate = false)
    {
        var options = new DbContextOptionsBuilder<DataFlowContext>()
            .UseNpgsql(connectionString)
            .Options;

        var dataFlowContext = new DataFlowContext(options, lockId);

        if (autoMigrate)
        {
            dataFlowContext.Database.EnsureCreated();
        }

        return dataFlowContext;
    }

    public static DataFlowContext CreatePostgres(IConfiguration configuration, string lockId)
    {
        if (configuration == null)
        {
            throw new ArgumentException("configuration was null. Please pass the configuration to the factory's constructor.");
        }

        var options = new DbContextOptionsBuilder<DataFlowContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;


        var dataFlowContext = new DataFlowContext(options, lockId);

        if (bool.TryParse(configuration.GetSection("Database:AutoMigrate").Value, out var result) && result)
        {
            dataFlowContext.Database.EnsureCreated();
        }

        return dataFlowContext;
    }

    public static DataFlowContext CreateInMem(string lockId, string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var context = new DataFlowContext(new DbContextOptionsBuilder<DataFlowContext>()
            .UseInMemoryDatabase(dbName)
            .Options, lockId);

        return context;
    }
}