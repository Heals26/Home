namespace Home.Application.Services.Persistence;

public interface IAuditPersistenceContext
{

    #region Properties

    #endregion Properties

    #region Methods

    void Add<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    #endregion Methods

}
