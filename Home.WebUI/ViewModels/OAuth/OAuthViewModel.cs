using System.Text.Json.Serialization;

namespace Home.WebUI.ViewModels.OAuth;

public class OAuthViewModel
{

    #region Properties

    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("claims")]
    public List<string> Claims { get; set; } = [];

    [JsonPropertyName("expiresIn")]
    public required long ExpiresIn { get; set; }

    [JsonPropertyName("grantType")]
    public required string GrantType { get; set; }

    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public required long UserID { get; set; }

    #endregion Properties

}
