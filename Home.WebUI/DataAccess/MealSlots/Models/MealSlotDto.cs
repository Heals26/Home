namespace Home.WebUI.DataAccess.MealSlots.Models;

public class MealSlotDto
{

    #region Properties

    /// <summary>
    /// The ID of the meal.
    /// </summary>
    public long MealSlotID { get; set; }

    /// <summary>
    /// What the household calls this meal.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order through the day.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Roughly when this meal happens, used to decide which meal the dashboard leads with.
    /// Null when the household hasn't said.
    /// </summary>
    public TimeSpan? StartsAt { get; set; }

    #endregion Properties

}
