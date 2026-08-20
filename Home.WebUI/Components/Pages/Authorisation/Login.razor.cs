using Home.WebUI.DataAccess.Households.GetSetupStatus;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class Login
{

    #region Fields

    private readonly CancellationTokenHandler m_CancellationTokenHandler = new();
    private string? m_Error;
    private bool m_RequiresSetup;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Supplied on the static render. Null means the interactive router got here first, and
    /// initialisation re-enters over plain HTTP.
    /// </summary>
    [CascadingParameter] public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromForm(FormName = "sign-in")] public string? Password { get; set; }
    [SupplyParameterFromQuery(Name = "returnUrl")] public string? ReturnUrl { get; set; }
    [SupplyParameterFromForm(FormName = "sign-in")] public string? Username { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        if (this.HttpContext == null)
        {
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
            return;
        }

        // A fresh install has nobody to sign in — quietly offer first-run setup instead.
        // Any failure here just leaves the link hidden; the form still works.
        var _Status = await this.ApiAccess.SendRequestAsync<object, GetSetupStatusWebAppResponse>(
            null!, ApiProvider.GetSetupStatus(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_RequiresSetup = _Status?.RequiresSetup == true;
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task SignInAsync()
    {
        if (this.HttpContext == null)
            return;

        if (string.IsNullOrWhiteSpace(this.Username) || string.IsNullOrWhiteSpace(this.Password))
        {
            this.m_Error = "Enter your username and password.";
            return;
        }

        if (this.LoginThrottle.GetLockout(this.Username) is { } _Lockout)
        {
            this.m_Error = $"Too many attempts. Wait about {Math.Max(1, (int)Math.Ceiling(_Lockout.TotalMinutes))} minute(s) and try again.";
            return;
        }

        var _Grant = await this.OAuthClient.TryPasswordGrantAsync(this.Username, this.Password, this.m_CancellationTokenHandler.Token);

        if (_Grant == null)
        {
            this.LoginThrottle.RecordFailure(this.Username);
            this.m_Error = "That username and password didn't match. Try again.";
            return;
        }

        this.LoginThrottle.RecordSuccess(this.Username);

        await this.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            SignInLogic.BuildPrincipal(_Grant.UserID, this.Username, _Grant.RefreshToken, _Grant.Claims),
            // Persistent is the point: without it the cookie dies with the browser, and closing
            // a tab would cost a password.
            new AuthenticationProperties() { IsPersistent = true });

        this.NavigationManager.NavigateTo(SignInLogic.GetSafeReturnUrl(this.ReturnUrl));
    }

    #endregion Methods

}
