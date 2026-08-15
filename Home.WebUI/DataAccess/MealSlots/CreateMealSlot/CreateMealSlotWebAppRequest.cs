namespace Home.WebUI.DataAccess.MealSlots.CreateMealSlot;

public class CreateMealSlotWebAppRequest
{

    #region Properties

    /// <summary>
    /// What the household calls this meal.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Roughly when this meal happens. Null when the household hasn't said.
    /// </summary>
    public TimeSpan? StartsAt { get; set; }

    #endregion Properties

}
