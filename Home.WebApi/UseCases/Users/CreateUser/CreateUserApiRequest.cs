namespace Home.WebApi.UseCases.Users.CreateUser;

public class CreateUserApiRequest
{

    #region Properties

    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleNames { get; set; } = string.Empty;
    public string? Password { get; set; }

    #endregion Properties

}
