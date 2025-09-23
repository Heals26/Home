using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Home.WebUI.DataAccess.OAuth;

public class OAuthWebAppRequest
{

    #region Properties

    [FromForm(Name = "client_id")]
    [JsonPropertyName("client_id")]
    public long ClientID { get; set; }

    [FromForm(Name = "client_secret")]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [FromForm(Name = "grant_type")]
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [FromForm(Name = "password")]
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [FromForm(Name = "refresh_token")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [FromForm(Name = "scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [FromForm(Name = "username")]
    [JsonPropertyName("username")]
    public string Username { get; set; }

    #endregion Properties

}
