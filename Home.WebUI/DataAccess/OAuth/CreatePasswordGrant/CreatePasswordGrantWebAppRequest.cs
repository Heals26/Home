using System.Text.Json.Serialization;

namespace Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;

public class CreatePasswordGrantWebAppRequest
{

    #region Properties

    [JsonPropertyName("client_id")]
    public long ClientID { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    #endregion Properties

}
