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
    /// Nullable because audits written without an authenticated user store no name, matching the
    /// optional column in <c>AuditConfiguration</c>.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Null once the member has been removed — the audit outlives them, and
    /// <see cref="UserName"/> keeps the record readable.
    /// </summary>
    public User? User { get; set; }

    #endregion Properties

}
