using Home.Application.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Home.Persistence.Database;

public class PersistenceContext(DbContextOptions<PersistenceContext> options) : DbContext(options), IPersistenceContext
{

    #region Constructors

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => _ = modelBuilder.ApplyConfigurationsFromAssembly(Application.AssemblyUtility.GetAssembly());

    #endregion Constructors

    #region Methods

    void IPersistenceContext.Add<TEntity>(TEntity entity)
        => base.Add(entity);

    void IPersistenceContext.AddRange<TEntity>(ICollection<TEntity> entities)
        => base.AddRange(entities);

    EntityEntry IPersistenceContext.Entity<TEntity>(TEntity entity)
        => base.Entry(entity);

    TEntity IPersistenceContext.Find<TEntity>(object entityID, params object[] additionalEntityIDs)
        => base.Find<TEntity>([entityID, .. additionalEntityIDs]);

    IQueryable<TEntity> IPersistenceContext.GetEntities<TEntity>()
        => this.Set<TEntity>().AsQueryable();

    void IPersistenceContext.Remove<TEntity>(TEntity entity)
        => base.Remove(entity);

    void IPersistenceContext.RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        => base.RemoveRange(entities);

    Task<int> IPersistenceContext.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    #endregion Methods

}
