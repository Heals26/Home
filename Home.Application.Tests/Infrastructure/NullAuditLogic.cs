using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.Tests.Infrastructure;

/// <summary>
/// Writes no history. Handed out by <see cref="TestServiceFactory"/> for any audit logic a test did
/// not register, so a slice test is about the slice.
/// </summary>
internal sealed class NullAuditLogic<TEntity> : IAuditLogic<TEntity> where TEntity : class
{

    #region Methods

    public void AddAudit(TEntity entity)
    {
    }

    public void AddAudit(TEntity entity, string summary)
    {
    }

    public void DeleteAudit(TEntity entity)
    {
    }

    public IQueryable<Audit> GetAudits()
        => Array.Empty<Audit>().AsQueryable();

    public void UpdateAudit(TEntity entity)
    {
    }

    public void UpdateAudit(TEntity entity, string summary)
    {
    }

    #endregion Methods

}
