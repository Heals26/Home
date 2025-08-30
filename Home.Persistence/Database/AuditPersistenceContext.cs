using Home.Application.Services.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Home.Persistence.Database;

public class AuditPersistenceContext(DbContextOptions<AuditPersistenceContext> options) : DbContext(options), IAuditPersistenceContext
{

    #region Methods

    void IAuditPersistenceContext.Add<TEntity>(TEntity entity)
        => base.Add(entity);

    Task<int> IAuditPersistenceContext.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    #endregion Methods

}
