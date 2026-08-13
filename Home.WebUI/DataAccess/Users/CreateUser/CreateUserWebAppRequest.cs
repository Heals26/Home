namespace Home.WebUI.DataAccess.Users.CreateUser;

public class CreateUserWebAppRequest
{

    #region Properties

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleNames { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    #endregion Properties

}
