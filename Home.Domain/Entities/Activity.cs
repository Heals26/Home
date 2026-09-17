using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class Activity : ISoftDeletable
{
    #region Fields

    private readonly ICollection<Audit> m_Audits = [];

    #endregion Fields

    #region Properties

    public long ActivityID { get; set; }

    /// <summary>
    /// Who ticked it off, or null when it is not done or was completed by a column move that no
    /// one person made. Set from the signed-in member, never from the request.
    /// </summary>
    public User? CompletedByUser { get; set; }

    public DateTime? CompletedDateUTC { get; set; }
    public DateTime? DeletedOnUTC { get; set; }
    public DateTime? DueDateUTC { get; set; }

    /// <summary>
    /// Time of day the activity is due, or null when only the day matters. Kept separate from
    /// the date so "no time set" stays representable without a companion flag.
    /// </summary>
    public TimeSpan? DueTime { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Tracking of who did what
    /// </summary>
    public ICollection<Audit> Audits { get; set; } = [];

    public Household Household { get; set; } = null!;

    /// <summary>
    /// The region where the content sits
    /// </summary>
    public ICollection<ActivityRegion> Regions { get; set; } = [];

    /// <summary>
    /// Todo, Refining, Progressing, Blocked, Testing, Done
    /// </summary>
    public ActivityState? State { get; set; }

    /// <summary>
    /// Where the card sits within its column. Ordering is per-column in practice: a card keeps
    /// its number when it moves, which is harmless because the order only has to be stable and
    /// rearrangeable, not gapless.
    /// </summary>
    public int Sequence { get; set; }

    public ICollection<ActivityTag> Tags { get; set; } = [];

    public User? User { get; set; }

    #endregion Properties

}
