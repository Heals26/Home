namespace Home.WebUI.DataAccess.MealPlanEntries.CreateMealPlanEntry;

/// <summary>
/// Plans one thing for one day. Send a <see cref="RecipeID"/> for something the household cooks,
/// or a <see cref="Title"/> for an occasion instead. One or the other.
/// </summary>
public class CreateMealPlanEntryWebAppRequest
{

    #region Properties

    /// <summary>
    /// The local calendar day to plan for.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Which meal of the day it is for. Null when the household has no meals defined.
    /// </summary>
    public long? MealSlotID { get; set; }

    /// <summary>
    /// The ID of the recipe to plan, or null when planning an occasion.
    /// </summary>
    public long? RecipeID { get; set; }

    /// <summary>
    /// What to call the entry when there is no recipe behind it.
    /// </summary>
    public string? Title { get; set; }

    #endregion Properties

}
