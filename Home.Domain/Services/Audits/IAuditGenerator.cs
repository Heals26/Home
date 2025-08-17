using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Domain.Services.Audits
{

    public interface IAuditGenerator<TEntity> where TEntity : class
    {

        #region Methods

        void AddAudit(TEntity entity, EntityState entityState, User user);

        #endregion Methods

    }

}
