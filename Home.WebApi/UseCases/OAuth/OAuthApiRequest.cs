using System.Text.Json.Serialization;

namespace Home.WebApi.UseCases.OAuth;

public class OAuthApiRequest
{

    #region Properties

    [JsonPropertyName("client_id")]
    public long ClientID { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    #endregion Properties

}
