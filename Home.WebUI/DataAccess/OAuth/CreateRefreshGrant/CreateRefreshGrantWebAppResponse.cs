namespace Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantWebAppResponse
{

    #region Properties

    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string GrantType { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public long UserID { get; set; }

    #endregion Properties

}
