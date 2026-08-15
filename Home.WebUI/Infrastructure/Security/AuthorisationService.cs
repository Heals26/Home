using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Home.WebUI.ViewModels.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.Security;

public class AuthorisationService(
    IServiceProvider serviceProvider,
    ProtectedLocalStorage protectedLocalStorage,
    TimeProvider timeProvider)
    : AuthenticationStateProvider, IAuthorisationService
{

    #region Fields

    /// <summary>
    /// The most recently published state. Starts null so reads fall back to the pending
    /// initialisation task, and is replaced on every sign-in, refresh and sign-out so a
    /// later read never sees a stale snapshot.
    /// </summary>
    private Task<AuthenticationState>? m_CurrentState;

    /// <summary>
    /// Remains incomplete until <see cref="InitialiseAsync"/> is called from
    /// <c>OnAfterRenderAsync</c> — the first point at which JS interop is available.
    /// <see cref="AuthorizeRouteView"/> shows its <c>Authorizing</c> slot while this
    /// task is pending, preventing any redirect to the login page.
    /// </summary>
    private readonly TaskCompletionSource<AuthenticationState> m_InitTcs = new();

    /// <summary>
    /// How far ahead of expiry a token counts as spent, so a request that is already in flight
    /// cannot outlive the token it was sent with.
    /// </summary>
    private static readonly TimeSpan s_RefreshWindow = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Caps the startup refresh. Without it an unreachable API would hold the
    /// <c>Authorizing</c> spinner open for the whole HTTP timeout.
    /// </summary>
    private static readonly TimeSpan s_StartupRefreshTimeout = TimeSpan.FromSeconds(10);

    #endregion Fields

    #region Methods

    private static AuthenticationState BuildAuthState(OAuthViewModel token)
    {
        var _Claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, token.UserID.ToString())
        };

        if (token.Claims != null)
            _Claims.AddRange(token.Claims.Select(c => new Claim(c, c)));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(_Claims, "Bearer")));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => this.m_CurrentState ?? this.m_InitTcs.Task;

    private DateTime GetExpiryUTC(long expiresInSeconds)
        => timeProvider.GetUtcNow().UtcDateTime.AddSeconds(expiresInSeconds);

    Task<OAuthViewModel?> IAuthorisationService.GetTokenAsync()
        => this.ReadTokenFromStorageAsync();

    /// <summary>
    /// Called once from <c>OnAfterRenderAsync(firstRender: true)</c> — at that point
    /// the Blazor circuit is live and <see cref="ProtectedLocalStorage"/> can safely
    /// perform JS interop to read from the browser.
    /// </summary>
    public async Task InitialiseAsync()
    {
        if (this.m_InitTcs.Task.IsCompleted)
            return;

        var _Token = await this.ReadTokenFromStorageAsync();

        if (_Token != null && this.IsExpiring(_Token))
            _Token = await this.RefreshAtStartupAsync(_Token);

        var _State = _Token != null ? BuildAuthState(_Token) : Unauthenticated();

        this.m_CurrentState = Task.FromResult(_State);
        this.m_InitTcs.SetResult(_State);
    }

    private bool IsExpiring(OAuthViewModel token)
        => token.ExpiresAtUTC <= timeProvider.GetUtcNow().UtcDateTime.Add(s_RefreshWindow);

    async Task<bool> IAuthorisationService.IsTokenExpiredAsync()
    {
        var _Token = await this.ReadTokenFromStorageAsync();

        return _Token != null && this.IsExpiring(_Token);
    }

    private void PublishState(AuthenticationState state)
    {
        this.m_CurrentState = Task.FromResult(state);
        this.NotifyAuthenticationStateChanged(this.m_CurrentState);
    }

    private async Task<OAuthViewModel?> ReadTokenFromStorageAsync()
    {
        try
        {
            var _Result = await protectedLocalStorage.GetAsync<string>(AuthorisationValues.OAuthKey);

            if (!_Result.Success || string.IsNullOrEmpty(_Result.Value))
                return null;

            return JsonSerializer.Deserialize<OAuthViewModel>(_Result.Value);
        }
        catch
        {
            // Stored value is unreadable (e.g. the data protection key ring changed) — treat as
            // unauthenticated and clear the stale entry so future reads succeed.
            try { await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey); } catch { }
            return null;
        }
    }

    /// <summary>
    /// A tablet coming back after the browser was closed almost always arrives with an access
    /// token that expired in the meantime. Spending the refresh token here — before the
    /// initialisation task completes — keeps <see cref="AuthorizeRouteView"/> in its
    /// <c>Authorizing</c> slot rather than bouncing the family to the login page.
    /// </summary>
    /// <returns>The token to publish, or null when the API has explicitly refused the session.</returns>
    private async Task<OAuthViewModel?> RefreshAtStartupAsync(OAuthViewModel token)
    {
        try
        {
            using var _Timeout = new CancellationTokenSource(s_StartupRefreshTimeout, timeProvider);

            // Resolved here rather than injected: HomeHttpClient takes an IAuthorisationService,
            // so constructor injection either way round would be a cycle.
            var _Outcome = await serviceProvider.GetRequiredService<HomeHttpClient>()
                .RefreshAccessTokenAsync(_Timeout.Token);

            if (_Outcome == TokenRefreshOutcome.Rejected)
            {
                await ((IAuthorisationService)this).SignOutAsync();
                return null;
            }

            return _Outcome == TokenRefreshOutcome.Refreshed
                ? await this.ReadTokenFromStorageAsync() ?? token
                : token;
        }
        catch
        {
            // Nothing that goes wrong reaching the API may cost the family its session.
            return token;
        }
    }

    async ValueTask IAuthorisationService.SignOutAsync()
    {
        try { await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey); } catch { }
        this.PublishState(Unauthenticated());
    }

    async Task<bool> IAuthorisationService.TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        var _Existing = await this.ReadTokenFromStorageAsync();

        return await this.TryStoreTokenAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = _Existing?.Claims ?? [],
            ExpiresAtUTC = this.GetExpiryUTC(response.ExpiresIn),
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });
    }

    async Task<bool> IAuthorisationService.TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken)
        => await this.TryStoreTokenAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = response.Claims,
            ExpiresAtUTC = this.GetExpiryUTC(response.ExpiresIn),
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });

    /// <summary>
    /// State is only published once the write has actually landed, so a storage failure leaves
    /// the previous session in place rather than a principal with no token behind it.
    /// </summary>
    private async Task<bool> TryStoreTokenAsync(OAuthViewModel token)
    {
        try
        {
            await protectedLocalStorage.SetAsync(AuthorisationValues.OAuthKey, JsonSerializer.Serialize(token));
        }
        catch
        {
            return false;
        }

        this.PublishState(BuildAuthState(token));
        return true;
    }

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    #endregion Methods

}
