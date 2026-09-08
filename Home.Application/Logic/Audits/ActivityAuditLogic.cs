using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ActivityAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<Activity>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.Activity;

    #endregion Properties

    #region Methods

    /// <summary>
    /// A chore's history is mostly about being done, so completion is read first; a move between
    /// columns that is not a completion comes next, then a rename or a change of hands.
    /// </summary>
    protected override string Describe(Activity activity, EntityState entityState)
    {
        var _Name = Quote(activity.Title);

        if (entityState == EntityState.Added)
            return $"added the chore {_Name}";

        if (entityState == EntityState.Deleted)
            return $"removed the chore {_Name}";

        if (this.HasChanged(activity, nameof(Activity.CompletedDateUTC)))
            return activity.CompletedDateUTC == null ? $"reopened {_Name}" : $"ticked off {_Name}";

        if (this.HasChanged(activity, "StateID", "ActivityStateID"))
            return activity.State == null ? $"took {_Name} off the board" : $"moved {_Name} to {activity.State.Name}";

        if (this.HasChanged(activity, nameof(Activity.Title)))
            return $"renamed {Quote(this.OriginalValue(activity, nameof(Activity.Title)) as string)} to {_Name}";

        if (this.HasChanged(activity, "UserID"))
            return activity.User == null ? $"unassigned {_Name}" : $"gave {_Name} to {activity.User.FirstName}";

        if (this.HasChanged(activity, nameof(Activity.DueDateUTC), nameof(Activity.DueTime)))
            return activity.DueDateUTC == null ? $"cleared the date on {_Name}" : $"moved {_Name} to {activity.DueDateUTC:ddd d MMM}";

        return $"changed {_Name}";
    }

    #endregion Methods

}
