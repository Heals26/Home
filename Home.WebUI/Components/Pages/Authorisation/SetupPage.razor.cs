using Home.WebUI.DataAccess.Households.GetSetupStatus;
using Home.WebUI.DataAccess.Households.RegisterHousehold;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class SetupPage
{

    #region Fields

    private readonly CancellationTokenHandler m_CancellationTokenHandler = new();
    private string? m_Error;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Supplied on the static render. Null means the interactive router got here first, and
    /// initialisation re-enters over plain HTTP.
    /// </summary>
    [CascadingParameter] public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromForm(FormName = "setup")] public string? Email { get; set; }
    [SupplyParameterFromForm(FormName = "setup")] public string? FirstName { get; set; }
    [SupplyParameterFromForm(FormName = "setup")] public string? HouseholdName { get; set; }
    [SupplyParameterFromForm(FormName = "setup")] public string? LastName { get; set; }
    [SupplyParameterFromForm(FormName = "setup")] public string? Password { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    // Registration only exists while the server has no users, so anyone landing here
    // after setup has happened is sent to the normal sign-in.
    protected override async Task OnInitializedAsync()
    {
        if (this.HttpContext == null)
        {
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
            return;
        }

        var _Status = await this.ApiAccess.SendRequestAsync<object, GetSetupStatusWebAppResponse>(
            null!, ApiProvider.GetSetupStatus(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        if (_Status == null || !_Status.RequiresSetup)
            this.NavigationManager.NavigateTo("/login");
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task RegisterAsync()
    {
        if (this.HttpContext == null)
            return;

        if (string.IsNullOrWhiteSpace(this.HouseholdName)
            || string.IsNullOrWhiteSpace(this.FirstName)
            || string.IsNullOrWhiteSpace(this.LastName)
            || string.IsNullOrWhiteSpace(this.Email)
            || string.IsNullOrWhiteSpace(this.Password))
        {
            this.m_Error = "Fill everything in — the household needs all of it.";
            return;
        }

        var _Request = new RegisterHouseholdWebAppRequest()
        {
            Email = this.Email,
            FirstName = this.FirstName,
            HouseholdName = this.HouseholdName,
            LastName = this.LastName,
            Password = this.Password
        };

        var _Result = await this.ApiAccess.SendRequestAsync<RegisterHouseholdWebAppRequest, RegisterHouseholdWebAppResponse>(
            _Request, ApiProvider.RegisterHousehold(),
            e => this.m_Error = e.Detail ?? e.Title ?? "The household couldn't be created. Try again.",
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
            return;

        // Sign the first member straight in — landing on a login form right after typing
        // the same details would be pure friction.
        var _Grant = await this.OAuthClient.TryPasswordGrantAsync(this.Email, this.Password, this.m_CancellationTokenHandler.Token);

        if (_Grant == null)
        {
            this.NavigationManager.NavigateTo("/login");
            return;
        }

        await this.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            SignInLogic.BuildPrincipal(_Grant.UserID, this.Email, _Grant.RefreshToken, _Grant.Claims),
            new AuthenticationProperties() { IsPersistent = true });

        this.NavigationManager.NavigateTo("/");
    }

    #endregion Methods

}
