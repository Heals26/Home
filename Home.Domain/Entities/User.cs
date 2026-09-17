using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class User : ISoftDeletable
{

    #region Properties

    public long UserID { get; set; }
    public DateTime? DeletedOnUTC { get; set; }

    /// <summary>
    /// What the member signs in with. Null for a member who has no login: a child who is assigned
    /// things and named on events but never taps a password. Present only together with
    /// <see cref="Password"/>.
    /// </summary>
    public string? Email { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleNames { get; set; } = string.Empty;
    /// <summary>
    /// Hashed. Null for a member without a login. A member with a password can sign in; one
    /// without can only be acted for.
    /// </summary>
    public string? Password { get; set; }

    public DateTime? PasswordLastChanged { get; set; }
    public string UserName { get => this.GetFullName(); }

    /// <summary>
    /// Whether this member can sign in at all. Everything that acts needs a signed-in member;
    /// being assigned something does not.
    /// </summary>
    public bool HasLogin { get => !string.IsNullOrEmpty(this.Password); }

    public ICollection<Activity> AssignedActivities { get; set; } = [];
    public ICollection<Audit> Audits { get; set; } = [];
    public Household Household { get; set; } = null!;

    #endregion Properties

    #region Methods

    public string GetFullName() =>
        $"{this.FirstName}{(string.IsNullOrEmpty(this.MiddleNames) ? string.Empty : " " + this.MiddleNames)} {this.LastName}";

    #endregion Methods

}
