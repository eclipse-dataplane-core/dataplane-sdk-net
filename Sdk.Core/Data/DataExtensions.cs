using Microsoft.EntityFrameworkCore;

namespace Sdk.Core.Data;

public static class DataExtensions
{
    public static async Task AddOrUpdateAsync<T>(this DbSet<T> dbSet, T entity) where T : class
    {
        var entry = dbSet.Local.FirstOrDefault(e => e == entity);
        if (entry != null)
        {
            dbSet.Update(entity);
        }
        else
        {
            await dbSet.AddAsync(entity);
        }
    }
}