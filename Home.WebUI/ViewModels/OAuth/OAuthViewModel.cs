using System.Text.Json.Serialization;

namespace Home.WebUI.ViewModels.OAuth;

public class OAuthViewModel
{

    #region Properties

    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("claims")]
    public List<string> Claims { get; set; } = [];

    /// <summary>
    /// When the access token stops being accepted, in UTC. Persisted alongside the token because
    /// <see cref="ExpiresIn"/> is only meaningful at the instant the API answered — a browser that
    /// was closed for a day cannot work out from it whether the token is still good. Tokens stored
    /// before this existed deserialise to <see cref="DateTime.MinValue"/>, which reads as expired
    /// and therefore triggers a refresh rather than a sign-out.
    /// </summary>
    [JsonPropertyName("expiresAtUTC")]
    public DateTime ExpiresAtUTC { get; set; }

    [JsonPropertyName("expiresIn")]
    public required long ExpiresIn { get; set; }

    [JsonPropertyName("grantType")]
    public required string GrantType { get; set; }

    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("userID")]
    public required long UserID { get; set; }

    #endregion Properties

}
