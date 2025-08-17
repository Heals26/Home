namespace Home.Application.Services.Database
{

    public interface IPersistenceContext
    {

        #region Methods

        void Add<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(ICollection<TEntity> entities) where TEntity : class;
        TEntity Find<TEntity>(object entityID, params object[] additionalEntityIDs) where TEntity : class;
        IQueryable<TEntity> GetEntities<TEntity>() where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        #endregion Methods

    }

}
