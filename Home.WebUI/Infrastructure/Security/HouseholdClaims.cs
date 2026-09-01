using System.Security.Claims;

namespace Home.WebUI.Infrastructure.Security;

/// <summary>
/// What the sign-in cookie carries, and the one place that knows how to read it back.
/// <para>
/// The refresh token rides in a claim because a Blazor circuit has no <c>HttpContext</c> to read
/// authentication tokens from — claims arrive with the principal and are readable anywhere. The
/// cookie is encrypted by data protection and marked HttpOnly, so nothing in the browser can read
/// either.
/// </para>
/// </summary>
public static class HouseholdClaims
{

    #region Fields

    public const string RefreshToken = "home:refresh-token";
    public const string Scope = "home:scope";

    #endregion Fields

    #region Methods

    public static string? GetRefreshToken(ClaimsPrincipal? principal)
        => principal?.FindFirst(RefreshToken)?.Value;

    /// <summary>
    /// Who is signed in on this device. Null when the claim is missing or unreadable, which the
    /// caller must treat as "not this member" rather than as any particular member — a screen that
    /// guessed would let someone edit or remove the wrong person.
    /// </summary>
    public static long? GetUserID(ClaimsPrincipal? principal)
        => long.TryParse(principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var _UserID)
            ? _UserID
            : null;

    #endregion Methods

}
