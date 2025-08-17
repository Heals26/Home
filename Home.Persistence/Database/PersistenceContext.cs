using Home.Application.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace Home.Persistence.Database
{

    public class PersistenceContext : DbContext, IPersistenceContext
    {

        #region Fields



        #endregion Fields

        #region Constructors

        public PersistenceContext(DbContextOptions<PersistenceContext> options) : base(options) { }

        #endregion Constructors

        #region Methods

        void IPersistenceContext.Add<TEntity>(TEntity entity)
            => base.Add(entity);

        void IPersistenceContext.AddRange<TEntity>(ICollection<TEntity> entities)
            => base.AddRange(entities);

        TEntity IPersistenceContext.Find<TEntity>(object entityID, params object[] additionalEntityIDs)
            => base.Find<TEntity>(new[] { entityID }.Concat(additionalEntityIDs).ToArray());

        IQueryable<TEntity> IPersistenceContext.GetEntities<TEntity>()
            => this.Set<TEntity>().AsQueryable();

        void IPersistenceContext.Remove<TEntity>(TEntity entity)
            => base.Remove(entity);

        void IPersistenceContext.RemoveRange<TEntity>(IEnumerable<TEntity> entities)
            => base.RemoveRange(entities);

        #endregion Methods

    }

}
