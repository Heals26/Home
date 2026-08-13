namespace Home.Domain.Entities;

public class UserAuthentication
{

    #region Properties

    public long AuthenticationMetadataID { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime DateSetUTC { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;

    public ClientApplication ClientApplication { get; set; } = null!;
    public User User { get; set; } = null!;

    #endregion Properties

}
