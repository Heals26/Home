namespace Home.Domain.Entities;

public class AuthenticationMetadata
{

    #region Properties

    public long AuthenticationMetadataID { get; set; }
    public string AccessToken { get; set; }
    public DateTime DateSetUTC { get; set; }
    public string RefreshToken { get; set; }
    public string Scopes { get; set; }

    public ClientApplication ClientApplication { get; set; }
    public User User { get; set; }

    #endregion Properties

}
