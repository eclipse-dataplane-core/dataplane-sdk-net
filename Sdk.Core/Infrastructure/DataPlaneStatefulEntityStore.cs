using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Infrastructure;

public class DataPlaneStatefulEntityStore(string lockId) : InMemoryStatefulEntityStore<DataFlow>(lockId), IDataPlaneStore
{
}