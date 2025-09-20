using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ActivityAuditLogic(IAuthorisationService authorisationService, IPersistenceContext persistenceContext)
    : AuditBase<Activity>(authorisationService, persistenceContext)
{

    #region Methods

    protected override void AddEntity(Activity activity)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.Activity,
            EntityID = activity.ActivityID,
            Content = this.GetAuditChanges(activity, EntityState.Added),
            UserName = this.GetUser()?.UserName,
        });

    protected override void DeleteEntity(Activity entity)
        => persistenceContext.GetEntities<Audit>()
            .Where(a => a.Entity == ResourceTypeSE.Activity && a.EntityID == entity.ActivityID)
            .ToList()
            .ForEach(a => persistenceContext.Remove(a));

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    protected override void UpdateEntity(Activity entity)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.Activity,
            EntityID = entity.ActivityID,
            Content = this.GetAuditChanges(entity, EntityState.Modified),
            UserName = this.GetUser().UserName,
        });

    #endregion Methods

}
