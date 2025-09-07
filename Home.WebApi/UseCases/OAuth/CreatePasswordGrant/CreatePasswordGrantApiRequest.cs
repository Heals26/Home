namespace Home.WebApi.UseCases.OAuth.CreatePasswordGrant;

public class CreatePasswordGrantApiRequest
{

    #region Properties

    public long ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string GrantType { get; set; }
    public string Password { get; set; }
    public string Scope { get; set; }
    public string Username { get; set; }

    #endregion Properties

}
