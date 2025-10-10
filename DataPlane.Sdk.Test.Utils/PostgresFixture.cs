using DataPlane.Sdk.Core.Data;
using JetBrains.Annotations;
using Testcontainers.PostgreSql;

namespace DataPlane.Sdk.Test.Utils;

[UsedImplicitly]
public class PostgresFixture : IAsyncDisposable
{
    private static PostgreSqlContainer? _postgreSqlContainer;

    // this must be constant per fixture lifetime
    public readonly string LockId = "lock-" + Guid.NewGuid().ToString("N");

    public PostgresFixture()
    {
        Context = CreateDbContext();
    }

    public DataFlowContext Context { get; }

    public async ValueTask DisposeAsync()
    {
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.DisposeAsync();
            await _postgreSqlContainer.DisposeAsync();
        }

        await Context.DisposeAsync();
    }

    private DataFlowContext CreateDbContext()
    {
        const string dbName = "SdkApiTests";
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase(dbName)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5432, true)
            .Build();
        _postgreSqlContainer.StartAsync().Wait();

        var port = _postgreSqlContainer.GetMappedPublicPort(5432);
        // dynamically map port to avoid conflicts
        var ctx = DataFlowContextFactory.CreatePostgres($"Host=localhost;Port={port};Database={dbName};Username=postgres;Password=postgres", LockId);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}