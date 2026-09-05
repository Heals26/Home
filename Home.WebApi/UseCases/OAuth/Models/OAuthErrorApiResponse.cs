namespace Home.WebApi.UseCases.OAuth.Models;

/// <summary>
/// Why the token endpoint refused, in the shape RFC 6749 section 5.2 defines.
/// <para>
/// The code matters to the sign-in page. <c>invalid_client</c> means this installation's own
/// credentials were rejected, which nobody at the keyboard can fix by retyping, while
/// <c>invalid_grant</c> means the username and password were wrong. Answering both with an empty
/// 401 made a misconfigured install look exactly like a mistyped password, which cost an evening
/// on 3 Sep 2026. Neither code reveals whether a username exists.
/// </para>
/// </summary>
public class OAuthErrorApiResponse
{

    #region Properties

    /// <summary>
    /// One of the codes on <c>OAuthValues</c>: <c>invalid_client</c>, <c>invalid_grant</c> and so on.
    /// </summary>
    public string Error { get; set; }

    #endregion Properties

}
