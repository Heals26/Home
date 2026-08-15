namespace Home.Domain.Entities;

/// <summary>
/// A named eating occasion the household defines for itself — Breakfast, Lunch, Dinner, Snack,
/// or whatever this family actually calls them. One vocabulary serves two jobs: which meal a
/// planned recipe is for, and how the recipe book is filtered.
/// </summary>
public class MealSlot
{

    #region Properties

    public long MealSlotID { get; set; }

    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order through the day.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Roughly when this meal happens, used to decide which slot the dashboard leads with.
    /// Null when the household hasn't said.
    /// </summary>
    public TimeSpan? StartsAt { get; set; }

    public ICollection<RecipeMealSlot> Recipes { get; set; } = [];

    #endregion Properties

}
