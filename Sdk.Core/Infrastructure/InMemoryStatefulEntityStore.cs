using System.Collections.Concurrent;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Infrastructure;

public abstract class InMemoryStatefulEntityStore<TEntity>(string lockId) where TEntity : Identifiable
{
    private static readonly TimeSpan DefaultLeaseTime = TimeSpan.FromSeconds(60);
    private readonly ConcurrentDictionary<string, Lease> _leases = new();

    private ConcurrentDictionary<string, TEntity> Entities { get; } = new();

    public Task<TEntity?> FindByIdAsync(string id)
    {
        return Task.FromResult(Entities.GetValueOrDefault(id));
    }

    public Task<ICollection<TEntity>> NextNotLeased(int state, params Criterion[] criteria)
    {
        throw new NotImplementedException();
    }

    public Task<StatusResult<TEntity>> FindByIdAndLeaseAsync(string id)
    {
        if (!Entities.TryGetValue(id, out var entity))
        {
            return Task.FromResult(StatusResult<TEntity>.NotFound());
        }

        try
        {
            AcquireLease(id);
            return Task.FromResult(StatusResult<TEntity>.Success(entity));
        }
        catch (ArgumentException e)
        {
            return Task.FromResult(StatusResult<TEntity>.Conflict($"Entity {id} is already leased by another process: {e.Message}"));
        }
    }

    public Task SaveAsync(TEntity entity)
    {
        AcquireLease(entity.Id);

        Entities[entity.Id] = entity;
        FreeLease(entity.Id);

        return Task.CompletedTask;
    }

    private void FreeLease(string entityId)
    {
        _leases.Remove(entityId, out _);
    }

    private void AcquireLease(string entityId)
    {
        AcquireLease(entityId, lockId, DefaultLeaseTime);
    }

    private void AcquireLease(string entityId, string lockId, TimeSpan defaultLeaseTime)
    {
        if (!IsLeased(entityId) || IsLeased(entityId, lockId))
        {
            _leases.TryAdd(entityId, new Lease(lockId, DateTime.UtcNow.Millisecond, defaultLeaseTime.Milliseconds));
        }
        else
        {
            throw new ArgumentException("Cannot acquire lease, entity ${entityId} is already leased by another process.");
        }
    }

    private bool IsLeased(string entityId, string lockId)
    {
        return IsLeased(entityId) && _leases[entityId].LeasedBy == lockId;
    }

    private bool IsLeased(string entityId)
    {
        return _leases.ContainsKey(entityId) && !_leases[entityId].IsExpired(DateTime.UtcNow.Millisecond);
    }
}