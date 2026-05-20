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

public class AuthorisationService(ProtectedLocalStorage protectedLocalStorage)
    : AuthenticationStateProvider, IAuthorisationService
{

    #region Methods

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var _Token = await ((IAuthorisationService)this).GetTokenAsync();

            if (_Token == null || string.IsNullOrEmpty(_Token.AccessToken))
                return Unauthenticated();

            var _Claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _Token.UserID.ToString())
            };

            if (_Token.Claims != null)
                _Claims.AddRange(_Token.Claims.Select(c => new Claim(c, c)));

            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity(_Claims, "Bearer")));
        }
        catch
        {
            // ProtectedLocalStorage is unavailable during pre-render (no JS interop yet)
            return Unauthenticated();
        }
    }

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

    async Task<bool> IAuthorisationService.IsTokenExpiredAsync()
    {
        var _Token = await ((IAuthorisationService)this).GetTokenAsync();

        if (_Token == null)
            return false;

        var _ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(_Token.ExpiresIn);
        return DateTimeOffset.UtcNow > _ExpiresAt.AddMinutes(-5);
    }

    private ValueTask StoreTokensAsync(OAuthViewModel oauthToken)
        => protectedLocalStorage.SetAsync(AuthorisationValues.OAuthKey, JsonSerializer.Serialize(oauthToken));

    async ValueTask IAuthorisationService.SignOutAsync()
    {
        await protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Unauthenticated()));
    }

    async Task<bool> IAuthorisationService.TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        await this.StoreTokensAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = (await ((IAuthorisationService)this).GetTokenAsync())?.Claims!,
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });

        this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        return true;
    }

    async Task<bool> IAuthorisationService.TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        await this.StoreTokensAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = response.Claims,
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        });

        this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        return true;
    }

    #endregion Methods

}
