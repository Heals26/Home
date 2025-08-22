using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Domain.Services.Audits
{

    public interface IAuditLogic<TEntity> where TEntity : class
    {

        #region Methods

        void AddAudit(TEntity entity, EntityState entityState, User user);
        IQueryable<Audit> GetAudits();

        #endregion Methods

    }

}
