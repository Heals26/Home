namespace Home.WebUI.DataAccess.OAuth.Models;

/// <summary>
/// Why the token endpoint refused, in the shape RFC 6749 section 5.2 defines. Present on a 401 or
/// 400 from the token endpoint and nowhere else.
/// </summary>
public class OAuthErrorWebAppResponse
{

    #region Properties

    /// <summary>
    /// <c>invalid_client</c> when this installation's own credentials were rejected,
    /// <c>invalid_grant</c> or similar when the username and password were.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    #endregion Properties

}
