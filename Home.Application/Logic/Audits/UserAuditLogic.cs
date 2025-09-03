using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class UserAuditLogic(IPersistenceContext persistenceContext) : AuditBase<User>(persistenceContext)
{

    #region Methods

    protected override void AddEntity(User userEntity, EntityState entityState, User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.User,
            EntityID = userEntity.UserID,
            Content = this.GetAuditChanges(userEntity, entityState),
            UserName = user.UserName,
        });

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    #endregion Methods

}
