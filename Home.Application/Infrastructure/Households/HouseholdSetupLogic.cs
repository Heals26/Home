using Home.Application.Services.EntityLogic.Households;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Households;

public class HouseholdSetupLogic(IPersistenceContext persistenceContext) : IHouseholdSetupLogic
{

    #region Fields

    /// <summary>
    /// Board columns in the language a family uses. The last one is the completed column, which
    /// is what stamps an activity's completed date. These same names are backfilled by the
    /// migration that made columns household-owned, so the two must stay in step.
    /// </summary>
    private static readonly (string Name, bool IsComplete)[] s_DefaultColumns =
    [
        ("To do", false),
        ("Doing", false),
        ("Waiting on", false),
        ("Done", true),
    ];

    private static readonly (string Name, TimeSpan StartsAt)[] s_DefaultMealSlots =
    [
        ("Breakfast", new TimeSpan(7, 0, 0)),
        ("Lunch", new TimeSpan(12, 0, 0)),
        ("Dinner", new TimeSpan(18, 0, 0)),
        ("Snack", new TimeSpan(15, 0, 0)),
    ];

    #endregion Fields

    #region Methods

    void IHouseholdSetupLogic.SeedDefaults(Household household)
    {
        var _Sequence = 0;

        foreach (var (_Name, _IsComplete) in s_DefaultColumns)
        {
            var _State = new ActivityState()
            {
                Household = household,
                IsComplete = _IsComplete,
                Name = _Name,
                Sequence = _Sequence++
            };

            household.ActivityStates.Add(_State);
            persistenceContext.Add(_State);
        }

        _Sequence = 0;

        foreach (var (_Name, _StartsAt) in s_DefaultMealSlots)
        {
            var _MealSlot = new MealSlot()
            {
                Household = household,
                Name = _Name,
                Sequence = _Sequence++,
                StartsAt = _StartsAt
            };

            household.MealSlots.Add(_MealSlot);
            persistenceContext.Add(_MealSlot);
        }
    }

    #endregion Methods

}
