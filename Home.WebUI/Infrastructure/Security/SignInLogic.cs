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
    /// Only ever somewhere inside this app — honouring an absolute URL here would make the
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
