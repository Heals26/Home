namespace Home.Application.Services.Security;

public class AuthenticationMetadata
{

    #region Properties

    /// <summary>
    /// Which session row this request arrived on, so the signed-in devices screen can mark one row
    /// as the device you are reading it on. Null on the token endpoint, which authenticates a
    /// client rather than a session.
    /// </summary>
    public long? AuthenticationMetadataID { get; set; }

    public long? UserID { get; set; }
    public long ClientApplicationID { get; set; }
    public string? ClientName { get; set; }
    public string? Scopes { get; set; }

    #endregion Properties

}
