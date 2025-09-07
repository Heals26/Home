using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ActivityAuditLogic(IPersistenceContext persistenceContext) : AuditBase<Activity>(persistenceContext)
{

    #region Methods

    protected override void AddEntity(Activity activity, User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.Activity,
            EntityID = activity.ActivityID,
            Content = this.GetAuditChanges(activity, EntityState.Added),
            UserName = user.UserName,
        });

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    protected override void UpdateEntity(Activity entity, User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.Activity,
            EntityID = entity.ActivityID,
            Content = this.GetAuditChanges(entity, EntityState.Modified),
            UserName = user.UserName,
        });

    #endregion Methods

}
