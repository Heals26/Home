using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Home.Persistence.Database;

public class PersistenceContext(DbContextOptions<PersistenceContext> options) : DbContext(options), IPersistenceContext
{

    #region Methods

    void IPersistenceContext.Add<TEntity>(TEntity entity)
        => base.Add(entity);

    void IPersistenceContext.AddRange<TEntity>(ICollection<TEntity> entities)
        => base.AddRange(entities);

    bool IPersistenceContext.DoesEntityExist<TEntity>(long entityID)
        => base.Find<TEntity>([entityID]) != null;

    EntityEntry IPersistenceContext.Entity<TEntity>(TEntity entity)
        => base.Entry(entity);

    TEntity? IPersistenceContext.Find<TEntity>(object entityID, params object[] additionalEntityIDs) where TEntity : class
        => base.Find<TEntity>([entityID, .. additionalEntityIDs]);

    IQueryable<TEntity> IPersistenceContext.GetEntities<TEntity>()
        => this.Set<TEntity>().AsQueryable();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => _ = modelBuilder.ApplyConfigurationsFromAssembly(AssemblyUtility.GetAssembly());

    void IPersistenceContext.Remove<TEntity>(TEntity entity)
        => base.Remove(entity);

    void IPersistenceContext.RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        => base.RemoveRange(entities);

    /// <summary>
    /// Saves, then gives any history row written for a brand-new entity the ID the database just
    /// assigned. The row is written before the insert, when the entity has no ID, so without this a
    /// thing's own history could never find its first entry.
    /// </summary>
    async Task<int> IPersistenceContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        var _Pending = this.ChangeTracker.Entries<Audit>()
            .Where(e => e.State == EntityState.Added && e.Entity.Subject != null)
            .Select(e => e.Entity)
            .ToList();

        var _Count = await base.SaveChangesAsync(cancellationToken);

        if (_Pending.Count == 0)
            return _Count;

        foreach (var _Audit in _Pending)
        {
            _Audit.EntityID = ReadID(_Audit.Subject!);
            _Audit.Subject = null;
        }

        return _Count + await base.SaveChangesAsync(cancellationToken);
    }

    private static long ReadID(object subject)
    {
        var _Property = subject.GetType().GetProperty($"{subject.GetType().Name}ID");

        return _Property?.GetValue(subject) is long _ID ? _ID : 0;
    }

    #endregion Methods

}
