namespace Home.WebUI.DataAccess.Users.Models;

public class UserSummaryDto
{

    #region Properties

    /// <summary>
    /// The member's email address — how they sign in.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// First, middle and last names joined for display.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the member.
    /// </summary>
    public long UserID { get; set; }

    #endregion Properties

}
