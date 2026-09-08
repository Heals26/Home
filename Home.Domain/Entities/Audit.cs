using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

public class Audit
{

    #region Properties

    public long AuditID { get; set; }
    public string Content { get; set; } = string.Empty;
    public ResourceTypeSE Entity { get; set; } = null!;
    public long EntityID { get; set; }
    public DateTime ModifiedDateUTC { get; set; }

    /// <summary>
    /// What happened, in plain words and the past tense, without the doer: "ticked off 'Bins'".
    /// The feed puts the name in front. Null on rows written before 8 Sep 2026.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Nullable because audits written without an authenticated user store no name, matching the
    /// optional column in <c>AuditConfiguration</c>.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Null once the member has been removed — the audit outlives them, and
    /// <see cref="UserName"/> keeps the record readable.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// The entity an addition describes, held only until the row is saved. A new entity has no ID
    /// when its history is written, so the persistence context reads the ID off this after the
    /// insert and fills <see cref="EntityID"/> in. Never stored.
    /// </summary>
    public object? Subject { get; set; }

    /// <summary>
    /// Whose history this is. Null only on rows written before the household was recorded and
    /// whose member has since gone; those never appear in a feed.
    /// </summary>
    public Household? Household { get; set; }

    #endregion Properties

}
