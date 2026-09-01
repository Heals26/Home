using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.MealPlanEntries.UpdateMealPlanEntry;

public class UpdateMealPlanEntryWebAppRequest
{

    #region Properties

    /// <summary>
    /// The day the meal is planned for.
    /// </summary>
    public PropertyChangeTracker<DateTime> Date { get; set; }

    /// <summary>
    /// Which meal of the day it is for.
    /// </summary>
    public PropertyChangeTracker<long?> MealSlotID { get; set; }

    #endregion Properties

}