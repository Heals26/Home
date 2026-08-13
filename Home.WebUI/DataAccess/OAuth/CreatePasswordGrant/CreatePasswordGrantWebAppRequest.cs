using System.Text.Json.Serialization;

namespace Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;

public class CreatePasswordGrantWebAppRequest
{

    #region Properties

    [JsonPropertyName("client_id")]
    public long ClientID { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    #endregion Properties

}
