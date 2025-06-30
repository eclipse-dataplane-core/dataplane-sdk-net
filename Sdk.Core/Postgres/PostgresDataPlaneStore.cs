using Microsoft.EntityFrameworkCore;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Postgres;

public class PostgresDataPlaneStore(DataFlowContextFactory contextFactory) : IDataPlaneStore
{
    public async Task<DataFlow?> FindByIdAsync(string id)
    {
        await using var ctx = await contextFactory.Create();
        return await ctx.DataFlows.FirstOrDefaultAsync(df => df.Id == id);
    }

    public Task<ICollection<DataFlow>> NextNotLeased(int state, params Criterion[] criteria)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<DataFlow>> FindByIdAndLeaseAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(DataFlow dataFlow)
    {
        await using var ctx = await contextFactory.Create();
        if (await ctx.DataFlows.ContainsAsync(dataFlow))
        {
            ctx.DataFlows.Update(dataFlow);
        }
        else
        {
            await ctx.DataFlows.AddAsync(dataFlow);
        }

        await ctx.SaveChangesAsync();
    }
}