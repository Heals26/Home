using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Domain.Services.Audits
{

    public interface IAuditLogic<TEntity> where TEntity : class
    {

        #region Methods

        void AddAudit(TEntity entity, EntityState entityState, User user);
        IReadOnlyList<Audit> GetPendingAudits();

        #endregion Methods

    }

}
