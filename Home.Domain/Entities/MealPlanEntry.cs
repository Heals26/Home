namespace Home.Domain.Entities;

/// <summary>
/// Something planned for a calendar day, which is the "what's for dinner" answer.
/// <para>
/// Usually a recipe. Not always: a meal can be an occasion rather than something cooked, and
/// "Father's Day", "leftovers" or "out for dinner" belong on the week just as much as a recipe
/// does. Exactly one of <see cref="Recipe"/> and <see cref="Title"/> is set.
/// </para>
/// <para>
/// The household is held here rather than reached through the recipe, which is what it used to do.
/// An entry that is only a title has no recipe to be reached through, and every query that scopes
/// the planner to one household needs an owner that is always there.
/// </para>
/// </summary>
public class MealPlanEntry
{

    #region Properties

    public long MealPlanEntryID { get; set; }

    /// <summary>
    /// The local calendar day the meal is planned for. The time component is always midnight, so
    /// only the date is meaningful.
    /// </summary>
    public DateTime Date { get; set; }

    public Household Household { get; set; } = null!;

    /// <summary>
    /// Which meal of the day this is for. Null on entries planned before the household defined
    /// its meals.
    /// </summary>
    public MealSlot? MealSlot { get; set; }

    /// <summary>
    /// The recipe being cooked, or null when the entry is an occasion rather than a dish.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// What the entry is called when there is no recipe behind it. Null when there is one, because
    /// the recipe's own name is the answer and a copy of it would go stale the moment it is renamed.
    /// </summary>
    public string? Title { get; set; }

    #endregion Properties

}
