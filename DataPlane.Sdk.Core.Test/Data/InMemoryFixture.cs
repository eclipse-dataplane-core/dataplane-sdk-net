using DataPlane.Sdk.Core.Data;
using JetBrains.Annotations;

namespace DataPlane.Sdk.Core.Test.Data;

[UsedImplicitly]
public class InMemoryFixture
{
    // this must be constant per fixture lifetime
    public readonly string LockId = "lock-" + Guid.NewGuid().ToString("N");

    // this can be re-instantiated on every read access
    public DataFlowContext Context => DataFlowContextFactory.CreateInMem(LockId);
}