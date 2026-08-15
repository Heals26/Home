using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.MealSlots.UpdateMealSlot;

public class UpdateMealSlotWebAppRequest
{

    #region Properties

    /// <summary>
    /// What the household calls this meal.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Display order through the day.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    /// <summary>
    /// Roughly when this meal happens.
    /// </summary>
    public PropertyChangeTracker<TimeSpan?> StartsAt { get; set; }

    #endregion Properties

}
