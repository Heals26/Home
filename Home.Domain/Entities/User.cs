namespace Home.Domain.Entities;


public class User
{

    #region Properties

    public long UserID { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleNames { get; set; }
    public string Password { get; set; }
    public DateTime PasswordLastChanged { get; set; }
    public string UserName { get => this.GetFullName(); }

    public ICollection<Activity> AssignedActivities { get; set; }
    public ICollection<Audit> Audits { get; set; }

    #endregion Properties

    #region Methods

    public string GetFullName() =>
        $"{this.FirstName}{(string.IsNullOrEmpty(this.MiddleNames) ? string.Empty : " " + this.MiddleNames)} {this.LastName}";

    #endregion Methods

}
