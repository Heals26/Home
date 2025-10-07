using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Home.WebUI.Infrastructure.Security;

public class AuthorisationService(IHttpContextAccessor httpContextAccessor) : IAuthorisationService
{

    #region Methods

    async Task<bool> IAuthorisationService.TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext == null)
            return false;

        var _Ticket = await httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (_Ticket == null)
            return false;

        _Ticket.Properties!.Items[".token.access_token"] = response.AccessToken;
        _Ticket.Properties!.Items[".token.refresh_token"] = response.RefreshToken;
        _Ticket.Properties!.ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
        await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, _Ticket.Principal!, _Ticket.Properties);

        return true;
    }

    async Task<bool> IAuthorisationService.TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken)
    {
        var _Claims = response.Claims.Select(c => new Claim(c, c)).ToList();
        _Claims.Add(new Claim(ClaimTypes.Name, request.Username!)); // Fallback if not in claims

        var _Identity = new ClaimsIdentity(_Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var _Principal = new ClaimsPrincipal(_Identity);

        // Store tokens in auth properties (encrypted in cookie)
        var _AuthenticationProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn)
        };
        _AuthenticationProperties.Items.Add(".token.access_token", response.AccessToken);
        _AuthenticationProperties.Items.Add(".token.refresh_token", response.RefreshToken);
        _AuthenticationProperties.Items.Add(".token.expires_in", response.ExpiresIn.ToString());

        // Sign in (creates encrypted cookie)
        var _HttpContext = httpContextAccessor.HttpContext;
        if (_HttpContext != null)
        {
            await _HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, _Principal, _AuthenticationProperties);
            return true;
        }

        return false;
    }

    #endregion Methods
}
