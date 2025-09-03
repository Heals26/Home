using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
namespace Home.Application.Logic.Audits;

public class ActivityAuditLogic(IPersistenceContext persistenceContext) : AuditBase<Activity>(persistenceContext)
{

    #region Methods

    protected override void AddEntity(Activity activity, EntityState entityState, User user)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = user,
            Entity = ResourceTypeSE.Activity,
            EntityID = activity.ActivityID,
            Content = this.GetAuditChanges(activity, entityState),
            UserName = user.UserName,
        });

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    #endregion Methods

}
