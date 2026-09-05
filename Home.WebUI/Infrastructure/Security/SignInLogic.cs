using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Home.WebUI.Infrastructure.Security;

/// <summary>
/// The parts of signing a device in that the login page and first-run setup share.
/// </summary>
public static class SignInLogic
{

    #region Methods

    public static ClaimsPrincipal BuildPrincipal(long userID, string username, string refreshToken, IEnumerable<string> scopes)
    {
        var _Claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userID.ToString()),
            new(ClaimTypes.Name, username),
            new(HouseholdClaims.RefreshToken, refreshToken)
        };

        _Claims.AddRange(scopes.Select(s => new Claim(HouseholdClaims.Scope, s)));

        return new ClaimsPrincipal(new ClaimsIdentity(_Claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    /// <summary>
    /// What to put on screen when signing in did not work.
    /// <para>
    /// All three of these used to read "that username and password didn't match", including when
    /// the API was not running and when this installation's own client credentials were wrong.
    /// Telling somebody to retype a password that was never the problem is the worst answer of the
    /// three, and it cost an evening on 3 Sep 2026.
    /// </para>
    /// </summary>
    public static string DescribeFailure(TokenRefreshOutcome outcome)
        => outcome switch
        {
            TokenRefreshOutcome.Rejected => "That username and password didn't match. Try again.",
            TokenRefreshOutcome.ClientRejected => "This copy of Home isn't set up correctly, so it can't sign anyone in. Its client credentials don't match the server. See step 4 of the README.",
            _ => "Home can't reach the server at the moment. Check it's running, then try again."
        };

    /// <summary>
    /// Only ever somewhere inside this app. Honouring an absolute URL here would make the
    /// sign-in form an open redirect for anyone who can get someone to submit it.
    /// </summary>
    public static string GetSafeReturnUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl)
            && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            && returnUrl.StartsWith('/')
            && !returnUrl.StartsWith("//")
                ? returnUrl
                : "/";

    #endregion Methods

}
