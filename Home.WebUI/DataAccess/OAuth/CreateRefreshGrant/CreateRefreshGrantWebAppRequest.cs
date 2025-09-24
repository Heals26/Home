using System.Text.Json.Serialization;

namespace Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantWebAppRequest
{

    #region Properties

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("client_id")]
    public long ClientID { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    #endregion Properties

}
