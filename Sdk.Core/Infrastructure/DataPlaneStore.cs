using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Infrastructure;

public class DataPlaneStore : IDataPlaneStore
{
    public Task<DataFlow?> FindByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<DataFlow>> NextNotLeased(int state, params Criterion[] criteria)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<DataFlow>> FindByIdAndLeaseAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(DataFlow dataFlow)
    {
        throw new NotImplementedException();
    }
}