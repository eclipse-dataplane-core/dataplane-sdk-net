using Sdk.Core.Data;
using Sdk.Core.Infrastructure;

namespace Sdk.Api.Test.Fixtures;

/// <summary>
///     fixture class for DPS API controller tests
/// </summary>
public class InMemoryFixture : AbstractFixture
{
    public InMemoryFixture()
    {
        Context = DataFlowContextFactory.CreateInMem("test-leaser");
        var signalingService = new DataPlaneSignalingService(Context, Sdk, "test-runtime-id");
        InitializeFixture(Context, signalingService);
    }
}