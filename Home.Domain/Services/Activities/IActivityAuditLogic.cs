using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Domain.Services.Activities
{
    public interface IActivityAuditLogic<TEntity>
    {

        #region Methods

        void AddAudit(Activity activity, EntityState entityState, User user);

        #endregion Methods

    }

}
