using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Home.Application.Services.Database
{

    public interface IPersistenceContext
    {

        #region Methods

        void Add<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(ICollection<TEntity> entities) where TEntity : class;
        EntityEntry Entity<TEntity>(TEntity entity);
        TEntity Find<TEntity>(object entityID, params object[] additionalEntityIDs) where TEntity : class;
        IQueryable<TEntity> GetEntities<TEntity>() where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        #endregion Methods

    }

}
