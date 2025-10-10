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
    public InMemoryFixture()
    {
        Context = DataFlowContextFactory.CreateInMem("test-leaser");
        var signalingService = new DataPlaneSignalingService(Context, Sdk);
        InitializeFixture(Context, signalingService);
    }
}