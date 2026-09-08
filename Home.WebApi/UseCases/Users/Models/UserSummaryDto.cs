namespace Home.WebApi.UseCases.Users.Models;

public class UserSummaryDto
{

    #region Properties

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this member can sign in. A member without a login is someone to assign things to.
    /// </summary>
    public bool HasLogin { get; set; }

    public string LastName { get; set; } = string.Empty;
    public long UserID { get; set; }

    #endregion Properties

}
