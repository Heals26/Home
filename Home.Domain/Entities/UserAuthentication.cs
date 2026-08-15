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

    /// <summary>
    /// What the family would recognise this session as — the User-Agent at sign-in — so a
    /// future "signed-in devices" screen can name the tablet rather than a token ID.
    /// </summary>
    public string? DeviceLabel { get; set; }

    /// <summary>
    /// When the refresh token stops working. A household device is expected to stay signed in
    /// for months, so this is long and slides forward on use.
    /// </summary>
    public DateTime ExpiresOnUTC { get; set; }

    public DateTime? LastUsedOnUTC { get; set; }

    /// <summary>
    /// The row that replaced this one when the token was rotated. A refresh presented against
    /// an already-superseded row outside the grace window means the token leaked, and the whole
    /// chain is revoked. Deliberately not a foreign key — it would close a reference cycle.
    /// </summary>
    public long? SupersededByAuthenticationMetadataID { get; set; }

    public DateTime? SupersededOnUTC { get; set; }

    public User User { get; set; } = null!;

    #endregion Properties

}
