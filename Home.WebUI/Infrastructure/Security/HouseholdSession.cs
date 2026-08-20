using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Home.WebUI.Infrastructure.Security;

/// <summary>
/// Holds this circuit's access token in memory, minted from the refresh token the sign-in cookie
/// carries. Access tokens live an hour and are cheap to replace; the cookie is what keeps the
/// household signed in for months, and nothing here can end it — only the logout page's sign-out
/// can clear a cookie.
/// </summary>
public class HouseholdSession(
    AuthenticationStateProvider authenticationStateProvider,
    IHttpContextAccessor httpContextAccessor,
    IOAuthClient oAuthClient)
    : IHouseholdSession
{

    #region Fields

    private string? m_AccessToken;

    /// <summary>
    /// One refresh at a time. A page load fires several API calls in parallel and every one of
    /// them can come back 401; without this they would all ask the token endpoint at once.
    /// </summary>
    private readonly SemaphoreSlim m_RefreshGate = new(1, 1);

    #endregion Fields

    #region Methods

    async Task<string?> IHouseholdSession.GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (this.m_AccessToken != null)
            return this.m_AccessToken;

        _ = await this.RefreshAsync(null, cancellationToken);

        return this.m_AccessToken;
    }

    /// <summary>
    /// A static render is a plain HTTP request and carries its principal on the context; a
    /// circuit has no context, so the authentication state it was started from is the truth.
    /// </summary>
    private async Task<ClaimsPrincipal> GetPrincipalAsync()
        => httpContextAccessor.HttpContext is { } _HttpContext
            ? _HttpContext.User
            : (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

    Task<TokenRefreshOutcome> IHouseholdSession.RefreshAsync(string? spentAccessToken, CancellationToken cancellationToken)
        => this.RefreshAsync(spentAccessToken, cancellationToken);

    private async Task<TokenRefreshOutcome> RefreshAsync(string? spentAccessToken, CancellationToken cancellationToken)
    {
        var _RefreshToken = HouseholdClaims.GetRefreshToken(await this.GetPrincipalAsync());

        if (string.IsNullOrEmpty(_RefreshToken))
            return TokenRefreshOutcome.Rejected;

        try
        {
            await this.m_RefreshGate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return TokenRefreshOutcome.Unavailable;
        }

        try
        {
            // Somebody else refreshed while this caller queued, so their result stands.
            if (this.m_AccessToken != null && this.m_AccessToken != spentAccessToken)
                return TokenRefreshOutcome.Refreshed;

            var _Result = await oAuthClient.RefreshAsync(_RefreshToken, cancellationToken);

            if (_Result.Outcome == TokenRefreshOutcome.Refreshed)
                this.m_AccessToken = _Result.Token!.AccessToken;

            return _Result.Outcome;
        }
        finally
        {
            _ = this.m_RefreshGate.Release();
        }
    }

    #endregion Methods

}
