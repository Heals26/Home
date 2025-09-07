namespace Home.Application.Services.User;

public class AuthenticationMetadata
{

    #region Properties

    public long UserID { get; set; }
    public long ClientApplicationID { get; set; }
    public string ClientName { get; set; }
    public string Scopes { get; set; }

    #endregion Properties

}
