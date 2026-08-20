using Home.WebUI.Infrastructure.Values;

namespace Home.WebUI.Infrastructure.Services.Security;

/// <summary>
/// The access token for whoever this circuit belongs to. Identity comes from the sign-in cookie,
/// which the browser sends with the request that starts the circuit, so there is nothing to read
/// out of the browser and nothing that can fail and look like a signed-out family.
/// </summary>
public interface IHouseholdSession
{

    #region Methods

    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Trades the cookie's refresh token for a new access token. Passing the token that just came
    /// back 401 lets a caller that queued behind someone else's refresh take the result of it
    /// rather than asking for another.
    /// </summary>
    Task<TokenRefreshOutcome> RefreshAsync(string? spentAccessToken, CancellationToken cancellationToken);

    #endregion Methods

}
