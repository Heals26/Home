namespace Home.WebApi.UseCases.Users.Models;

public class UserSummaryDto
{

    #region Properties

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public long UserID { get; set; }

    #endregion Properties

}
