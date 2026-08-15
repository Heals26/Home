namespace Home.Domain.Entities;

/// <summary>
/// A column on a household's board. Owned by the household rather than shared globally, so one
/// family renaming "Doing" cannot rename it for everyone.
/// </summary>
public class ActivityState
{

    #region Properties

    public long ActivityStateID { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Activity> Activities { get; set; } = [];

    public Household Household { get; set; } = null!;

    /// <summary>
    /// Whether landing in this column means the activity is finished — this is what stamps
    /// <see cref="Activity.CompletedDateUTC"/>, so the dashboard stops listing done chores.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Left-to-right order on the board.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
