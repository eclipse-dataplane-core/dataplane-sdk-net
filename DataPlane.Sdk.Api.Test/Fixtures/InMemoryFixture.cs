using DataPlane.Sdk.Core.Data;
using DataPlane.Sdk.Core.Infrastructure;
using JetBrains.Annotations;

namespace DataPlane.Sdk.Api.Test.Fixtures;

/// <summary>
///     fixture class for DPS API controller tests
/// </summary>
[UsedImplicitly]
public class InMemoryFixture : AbstractFixture
{
    private static readonly Sdk.Test.Utils.InMemoryFixture RootFixture = new();

    // this must be constant per fixture lifetime, so a property accessor (=>) would not work, b/c that would potentially re-instantiate the context
    public readonly DataFlowContext Context = RootFixture.Context;

    public InMemoryFixture()
    {
        var signalingService = new DataPlaneSignalingService(Context, Sdk);
        InitializeFixture(Context, signalingService);
    }
}