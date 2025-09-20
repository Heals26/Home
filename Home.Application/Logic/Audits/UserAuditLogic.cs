using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class UserAuditLogic(IAuthorisationService authorisationService, IPersistenceContext persistenceContext)
    : AuditBase<User>(authorisationService, persistenceContext)
{

    #region Methods

    protected override void AddEntity(User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.User,
            EntityID = user.UserID,
            Content = this.GetAuditChanges(user, EntityState.Added),
            UserName = user.UserName,
        });

    protected override void DeleteEntity(User entity)
        => persistenceContext.GetEntities<Audit>()
            .Where(a => a.Entity == ResourceTypeSE.User && a.EntityID == entity.UserID)
            .ToList()
            .ForEach(a => persistenceContext.Remove(a));

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    protected override void UpdateEntity(User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.User,
            EntityID = user.UserID,
            Content = this.GetAuditChanges(user, EntityState.Modified),
            UserName = user.UserName
        });

    #endregion Methods

}
