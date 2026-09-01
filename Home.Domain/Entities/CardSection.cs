namespace Home.Domain.Entities;

/// <summary>
/// A heading a household puts on its activity cards — "Details", "Steps", whatever suits them.
/// <para>
/// These were fixed in code as Description / AcceptanceCriteria / Notes until 1 Sep 2026. Nobody
/// writes acceptance criteria for mowing the lawn, and a family card is not a software ticket, so
/// the sections belong to the household the same way its board columns and meal slots do.
/// </para>
/// </summary>
public class CardSection
{

    #region Properties

    public long CardSectionID { get; set; }

    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<ActivityRegion> Regions { get; set; } = [];

    /// <summary>
    /// The order the sections read down a card.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
