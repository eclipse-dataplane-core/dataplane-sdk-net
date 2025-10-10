using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Infrastructure;
using JetBrains.Annotations;

namespace DataPlane.Sdk.Api.Test.Fixtures;

[UsedImplicitly]
public class PostgresFixture : AbstractFixture
{
    private readonly Sdk.Test.Utils.PostgresFixture _postgresFixture = new();

    public PostgresFixture()
    {
        var signalingService = new DataPlaneSignalingService(Context, Sdk);
        InitializeFixture(Context, signalingService);
    }

    public DataFlowContext Context => _postgresFixture.Context;
}