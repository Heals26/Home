using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Home.WebUI.ViewModels.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.Security;

public class AuthorisationService(ProtectedLocalStorage protectedLocalStorage, TimeProvider timeProvider)
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

    async Task<OAuthViewModel?> IAuthorisationService.GetTokenAsync()
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
            // Stored value is unreadable (e.g. key ring changed) — treat as unauthenticated
            // and attempt to clear the stale entry so future reads succeed
            try { await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey); } catch { }
            return null;
        }
    }

    /// <summary>
    /// Called once from <c>OnAfterRenderAsync(firstRender: true)</c> — at that point
    /// the Blazor circuit is live and <see cref="ProtectedLocalStorage"/> can safely
    /// perform JS interop to read from the browser.
    /// </summary>
    public async Task InitialiseAsync()
    {
        if (this.m_InitTcs.Task.IsCompleted)
            return;

        this.m_InitTcs.SetResult(await this.ReadStateFromStorageAsync());
    }

    async Task<bool> IAuthorisationService.IsTokenExpiredAsync()
    {
        var _Token = await ((IAuthorisationService)this).GetTokenAsync();

        if (_Token == null)
            return false;

        return timeProvider.GetUtcNow() > DateTimeOffset.FromUnixTimeSeconds(_Token.ExpiresIn).AddMinutes(-5);
    }

    private void PublishState(AuthenticationState state)
    {
        this.m_CurrentState = Task.FromResult(state);
        this.NotifyAuthenticationStateChanged(this.m_CurrentState);
    }

    private async Task<AuthenticationState> ReadStateFromStorageAsync()
    {
        try
        {
            var _Result = await protectedLocalStorage.GetAsync<string>(AuthorisationValues.OAuthKey);

            if (!_Result.Success || string.IsNullOrEmpty(_Result.Value))
                return Unauthenticated();

            var _Token = JsonSerializer.Deserialize<OAuthViewModel>(_Result.Value);
            return _Token != null ? BuildAuthState(_Token) : Unauthenticated();
        }
        catch
        {
            try { await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey); } catch { }
            return Unauthenticated();
        }
    }

    async ValueTask IAuthorisationService.SignOutAsync()
    {
        await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey);
        this.PublishState(Unauthenticated());
    }

    private async Task StoreTokenAsync(OAuthViewModel token)
    {
        await protectedLocalStorage.SetAsync(AuthorisationValues.OAuthKey, JsonSerializer.Serialize(token));
        this.PublishState(BuildAuthState(token));
    }

    async Task<bool> IAuthorisationService.TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        await this.StoreTokenAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = (await ((IAuthorisationService)this).GetTokenAsync())?.Claims ?? [],
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });

        return true;
    }

    async Task<bool> IAuthorisationService.TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        await this.StoreTokenAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = response.Claims,
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });

        return true;
    }

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    #endregion Methods

}
