using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Domain.Logic.Audits
{

    public class AuditLogic : IAuditLogic
    {

        #region Fields

        private readonly IServiceProvider m_ServiceProvider;
        private readonly List<Audit> m_PendingAudits = new();

        #endregion Fields

        #region Constructors

        public AuditLogic(IServiceProvider serviceProvider)
            => this.m_ServiceProvider = serviceProvider;

        #endregion Constructors

        #region Methods

        void IAuditLogic.AddAudit<TEntity>(TEntity entity, EntityState entityState, User user) where TEntity : class
        {
            var _EntityLogic = (I)
            this.AddAudit(entity, entityState, user);
        }

        #endregion Methods

    }

}
