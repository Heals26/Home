using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Households.GetSetupStatus;
using Home.WebUI.DataAccess.Households.RegisterHousehold;
using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class SetupPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private RegisterHouseholdWebAppRequest m_Model = new();
    private bool m_CheckingStatus = true;
    private bool m_IsLoading;

    #endregion Fields

    #region Lifecycle Methods

    // Registration only exists while the server has no users, so anyone landing here
    // after setup has happened is sent to the normal sign-in.
    protected override async Task OnInitializedAsync()
    {
        var _Status = await this.ApiAccess.SendRequestAsync<object, GetSetupStatusWebAppResponse>(
            null!, ApiProvider.GetSetupStatus(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Status == null || !_Status.RequiresSetup)
        {
            this.NavigationManager.NavigateTo("/login");
            return;
        }

        this.m_CheckingStatus = false;
    }

    #endregion Lifecycle Methods

    #region Methods

    private bool CanSubmit()
        => !string.IsNullOrWhiteSpace(this.m_Model.HouseholdName)
            && !string.IsNullOrWhiteSpace(this.m_Model.FirstName)
            && !string.IsNullOrWhiteSpace(this.m_Model.LastName)
            && !string.IsNullOrWhiteSpace(this.m_Model.Email)
            && !string.IsNullOrWhiteSpace(this.m_Model.Password);

    private async Task HandleSubmitAsync()
    {
        if (this.m_IsLoading)
            return;

        this.m_IsLoading = true;

        var _Result = await this.ApiAccess.SendRequestAsync<RegisterHouseholdWebAppRequest, RegisterHouseholdWebAppResponse>(
            this.m_Model, ApiProvider.RegisterHousehold(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
        {
            this.m_IsLoading = false;
            return;
        }

        // Sign the first member straight in — landing on a login form right after
        // typing the same details would be pure friction.
        var _SignedIn = await this.ApiAccess.TryLoginAsync(
            new() { Username = this.m_Model.Email, Password = this.m_Model.Password },
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_IsLoading = false;

        this.NavigationManager.NavigateTo(_SignedIn ? "/" : "/login");
    }

    #endregion Methods

}
