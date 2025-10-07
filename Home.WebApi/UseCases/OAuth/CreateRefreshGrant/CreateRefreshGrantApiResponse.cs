namespace Home.WebApi.UseCases.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantApiResponse
{

    #region Properties

    public string AccessToken { get; set; }
    public long ExpiresIn { get; set; }
    public string GrantType { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
    public long UserID { get; set; }

    #endregion Properties

}
