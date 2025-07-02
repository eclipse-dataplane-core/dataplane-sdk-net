using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

namespace Sdk.Core.Data;

public class DataFlowContext : DbContext, IDataPlaneStore
{
    private static readonly TimeSpan DefaultLeaseTime = TimeSpan.FromSeconds(60);
    private readonly string _lockId;

    internal DataFlowContext(DbContextOptions<DataFlowContext> options, string lockId)
        : base(options)
    {
        _lockId = lockId;
    }

    public DbSet<DataFlow> DataFlows { get; set; }
    public DbSet<Lease> Leases { get; set; }

    public async Task SaveAsync(DataFlow df)
    {
        var lease = await AcquireLeaseAsync(df.Id);
        if (DataFlows.Contains(df))
        {
            DataFlows.Update(df);
        }
        else
        {
            DataFlows.Add(df);
        }

        FreeLeaseAsync(lease);
    }

    public async Task<DataFlow?> FindByIdAsync(string id)
    {
        var df = await DataFlows.FindAsync(id);
        return df;
    }

    public async Task<StatusResult<DataFlow>> FindByIdAndLeaseAsync(string id)
    {
        var flow = await FindByIdAsync(id);
        if (flow == null)
        {
            return StatusResult<DataFlow>.NotFound();
        }

        try
        {
            await AcquireLeaseAsync(flow.Id);
            return StatusResult<DataFlow>.Success(flow);
        }
        catch (ArgumentException e)
        {
            return StatusResult<DataFlow>.Conflict($"Entity {id} is already leased by another process: {e.Message}");
        }
    }

    public async Task<ICollection<DataFlow>> NextNotLeased(int max, params int[] states)
    {
        var filteredFlows = DataFlows.Where(dataFlow => states.Contains(dataFlow.State)).Take(max);
        return await filteredFlows.ToListAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataFlow>()
            .HasKey(df => df.Id);

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Id)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.State)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.CreatedAt)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.UpdatedAt)
            .IsRequired();
        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Source)
            .HasConversion(da => ToJson(da),
                s => JsonSerializer.Deserialize<DataAddress>(s, null as JsonSerializerOptions)!);

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Destination)
            .HasConversion(da => ToJson(da),
                s => JsonSerializer.Deserialize<DataAddress>(s, null as JsonSerializerOptions)!);

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.TransferType)
            .HasConversion(da => ToJson(da),
                s => JsonSerializer.Deserialize<TransferType>(s, null as JsonSerializerOptions)!);

        modelBuilder.Entity<Lease>()
            .HasKey(l => l.EntityId);
    }

    private static string ToJson(dynamic da)
    {
        return JsonSerializer.Serialize(da);
    }

    private void FreeLeaseAsync(Lease lease)
    {
        Leases.Remove(lease);
    }

    private async Task<Lease> AcquireLeaseAsync(string entityId)
    {
        return await AcquireLeaseAsync(entityId, _lockId, DefaultLeaseTime);
    }

    private async Task<Lease> AcquireLeaseAsync(string entityId, string lockId, TimeSpan leaseDuration)
    {
        var lease = new Lease
        {
            EntityId = entityId,
            LeasedBy = lockId,
            LeasedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LeaseDurationMillis = (long)leaseDuration.TotalMilliseconds
        };
        if (!await IsLeasedAsync(entityId))
        {
            await Leases.AddAsync(lease);
        }
        else if (await IsLeasedByAsync(entityId, lockId))
        {
            // load tracked entity and update its values
            var existing = await Leases.FindAsync(entityId);
            Entry(existing!).CurrentValues.SetValues(lease);
        }
        else
        {
            throw new ArgumentException("Cannot acquire lease, entity ${entityId} is already leased by another process.");
        }

        return lease;
    }

    private async Task<bool> IsLeasedByAsync(string entityId, string lockId)
    {
        var lease = await Leases.FindAsync(entityId);
        return lease != null && !lease.IsExpired(DateTime.UtcNow.Millisecond) && lease.LeasedBy == lockId;
    }

    private async Task<bool> IsLeasedAsync(string entityId)
    {
        var lease = await Leases.FindAsync(entityId);
        return lease != null && !lease.IsExpired(DateTime.UtcNow.Millisecond);
    }
}