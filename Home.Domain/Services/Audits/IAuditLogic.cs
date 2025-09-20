using Home.Domain.Entities;

namespace Home.Domain.Services.Audits;

public interface IAuditLogic<TEntity> where TEntity : class
{

    #region Methods

    /// <summary>
    /// The entity to be added
    /// </summary>
    /// <param name="entity">The relevant entity</param>
    /// <param name="user">The user that did the deed</param>
    void AddAudit(TEntity entity);

    /// <summary>
    /// The entity to be deleted
    /// </summary>
    /// <param name="entity"></param>
    void DeleteAudit(TEntity entity);

    IQueryable<Audit> GetAudits();

    /// <summary>
    /// The entity to be updated
    /// </summary>
    /// <param name="entity">The relevant entity</param>
    /// <param name="user">The user that did the deed</param>
    void UpdateAudit(TEntity entity);

    #endregion Methods

}
