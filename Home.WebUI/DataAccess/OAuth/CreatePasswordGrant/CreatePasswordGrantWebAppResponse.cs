namespace Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;

public class CreatePasswordGrantWebAppResponse
{

    #region Properties
    public required string AccessToken { get; set; }
    public List<string> Claims { get; set; } = [];
    public required long ExpiresIn { get; set; }
    public required string GrantType { get; set; }
    public required string RefreshToken { get; set; }
    public string Scope { get; set; } = string.Empty;
    public required long UserID { get; set; }

    #endregion Properties

}
