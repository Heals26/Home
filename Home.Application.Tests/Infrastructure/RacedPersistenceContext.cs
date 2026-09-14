using Home.Application.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// A context that loses a race on its first save: another request's write lands just before it, and
/// the save fails the way SQL Server reports a unique index clash. The in-memory store has no unique
/// indexes, so the clash is thrown here instead of by the database.
/// </summary>
public sealed class RacedPersistenceContext(IPersistenceContext inner, Action otherRequestSaves) : IPersistenceContext
{

    #region Fields

    private bool m_Raced;

    #endregion Fields

    #region Methods

    public void Add<TEntity>(TEntity entity) where TEntity : class
        => inner.Add(entity);

    public void AddRange<TEntity>(ICollection<TEntity> entities) where TEntity : class
        => inner.AddRange(entities);

    public bool DoesEntityExist<TEntity>(long entityID) where TEntity : class
        => inner.DoesEntityExist<TEntity>(entityID);

    public EntityEntry Entity<TEntity>(TEntity entity) where TEntity : class
        => inner.Entity(entity);

    public TEntity? Find<TEntity>(object entityID, params object[] additionalEntityIDs) where TEntity : class
        => inner.Find<TEntity>(entityID, additionalEntityIDs);

    public IQueryable<TEntity> GetEntities<TEntity>() where TEntity : class
        => inner.GetEntities<TEntity>();

    public void Remove<TEntity>(TEntity entity) where TEntity : class
        => inner.Remove(entity);

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        => inner.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (this.m_Raced)
            return inner.SaveChangesAsync(cancellationToken);

        this.m_Raced = true;
        otherRequestSaves();

        throw new DbUpdateException("Cannot insert duplicate key row in object 'home.ShoppingItemMemory' with unique index 'IX_ShoppingItemMemory_HouseholdID_NameKey'.");
    }

    #endregion Methods

}
