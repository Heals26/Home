namespace Home.WebApi.UseCases.OAuth.CreateRefreshGrant;

public class CreateGrantApiRequest
{

    #region Properties

    public long ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string GrantType { get; set; }
    public string RefreshToken { get; set; }

    #endregion Properties

}
