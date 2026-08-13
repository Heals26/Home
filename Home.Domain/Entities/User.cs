namespace Home.Domain.Entities;

public class User
{

    #region Properties

    public long UserID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleNames { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime PasswordLastChanged { get; set; }
    public string UserName { get => this.GetFullName(); }

    public ICollection<Activity> AssignedActivities { get; set; } = [];
    public ICollection<Audit> Audits { get; set; } = [];
    public Household Household { get; set; } = null!;

    #endregion Properties

    #region Methods

    public string GetFullName() =>
        $"{this.FirstName}{(string.IsNullOrEmpty(this.MiddleNames) ? string.Empty : " " + this.MiddleNames)} {this.LastName}";

    #endregion Methods

}
