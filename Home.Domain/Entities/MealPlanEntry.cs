namespace Home.Domain.Entities;

/// <summary>
/// A recipe planned for a calendar day — the "what's for dinner" answer. The household is
/// reached through the recipe rather than a second foreign key, which keeps SQL Server to a
/// single cascade path (the same shape as <see cref="LightSchedule"/>).
/// </summary>
public class MealPlanEntry
{

    #region Properties

    public long MealPlanEntryID { get; set; }

    /// <summary>
    /// The local calendar day the meal is planned for. The time component is always midnight —
    /// only the date is meaningful.
    /// </summary>
    public DateTime Date { get; set; }

    public Recipe Recipe { get; set; } = null!;

    #endregion Properties

}
