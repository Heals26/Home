using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

/// <summary>
/// The planner's history, which is also the meal memory: "planned Tacos for Wednesday dinner".
/// Callers must have loaded the entry's recipe and meal slot, or the words go missing.
/// </summary>
public class MealPlanEntryAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<MealPlanEntry>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.MealPlanEntry;

    #endregion Properties

    #region Methods

    protected override string Describe(MealPlanEntry entry, EntityState entityState)
    {
        var _Meal = entry.Recipe?.Name ?? entry.Title ?? "a meal";
        var _When = entry.MealSlot == null ? $"{entry.Date:dddd}" : $"{entry.Date:dddd} {entry.MealSlot.Name.ToLowerInvariant()}";

        if (entityState == EntityState.Added)
            return $"planned {_Meal} for {_When}";

        if (entityState == EntityState.Deleted)
            return $"took {_Meal} off {_When}";

        return $"moved {_Meal} to {_When}";
    }

    #endregion Methods

}
