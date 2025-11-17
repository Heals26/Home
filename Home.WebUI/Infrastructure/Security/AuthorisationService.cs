using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Home.WebUI.ViewModels.OAuth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.Security;

public class AuthorisationService(ProtectedLocalStorage protectedLocalStorage) : IAuthorisationService
{

    #region Methods

    private JsonSerializerOptions GetOptions()
        => new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    async Task<OAuthViewModel?> IAuthorisationService.GetTokenAsync()
    {
        var _Result = await protectedLocalStorage.GetAsync<string>(AuthorisationValues.OAuthKey);
        if (!_Result.Success || string.IsNullOrEmpty(_Result.Value))
            return null;

        return JsonSerializer.Deserialize<OAuthViewModel>(_Result.Value);
    }

    async Task<bool> IAuthorisationService.IsTokenExpiredAsync()
    {
        var _Token = await ((IAuthorisationService)this).GetTokenAsync();

        if (_Token == null)
            return false;

        var _ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(_Token.ExpiresIn);
        return DateTimeOffset.UtcNow > _ExpiresAt.AddMinutes(-5);
    }

    private ValueTask StoreTokensAsync(OAuthViewModel oauthToken, JsonSerializerOptions options)
         => protectedLocalStorage.SetAsync(AuthorisationValues.OAuthKey, JsonSerializer.Serialize(oauthToken, options));

    ValueTask IAuthorisationService.SignOutAsync()
        => protectedLocalStorage.DeleteAsync(AuthorisationValues.OAuthKey);

    async Task<bool> IAuthorisationService.TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        // Store refreshed tokens in browser (no SignInAsync needed)
        await StoreTokensAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = (await ((IAuthorisationService)this).GetTokenAsync())?.Claims!,
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        }, GetOptions());

        return true;
    }

    async Task<bool> IAuthorisationService.TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        var _Claims = response.Claims.Select(c => new Claim(c, c)).ToList();
        _Claims.Add(new Claim(ClaimTypes.Name, request.Username!));

        await StoreTokensAsync(new()
        {
            AccessToken = response.AccessToken,
            Claims = response.Claims,
            ExpiresIn = response.ExpiresIn,
            GrantType = response.GrantType,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UserID = response.UserID
        }, this.GetOptions());

        return true;
    }

    #endregion Methods

}
