namespace Home.Domain.Entities;

/// <summary>
/// One household-defined section as it appears on one card, holding that card's lines for it.
/// </summary>
public class ActivityRegion
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public int Sequence { get; set; }

    public Activity Activity { get; set; } = null!;
    public CardSection CardSection { get; set; } = null!;
    public ICollection<ActivityContent> Fields { get; set; } = [];

    #endregion Properties

}
