using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Households.GetHouseholdSettings;
using Home.WebUI.DataAccess.Households.UpdateHouseholdSettings;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;

namespace Home.WebUI.Components.Pages.Settings;

public partial class SettingsPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private GetHouseholdSettingsWebAppResponse? m_Settings;

    // Household
    private string m_Name = string.Empty;
    private bool m_SavingHousehold;
    private bool m_HouseholdSaved;

    // Location
    private double? m_Latitude;
    private double? m_Longitude;
    private bool m_SavingLocation;
    private bool m_LocationSaved;

    // Connections
    private string m_LifxToken = string.Empty;
    private bool m_SavingConnection;
    private bool m_ConnectionSaved;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
        => await this.LoadSettingsAsync();

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadSettingsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetHouseholdSettingsWebAppResponse>(
            null!, ApiProvider.GetHouseholdSettings(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
            return;

        this.m_Settings = _Result;
        this.m_Name = _Result.Name;
        this.m_Latitude = _Result.Latitude;
        this.m_Longitude = _Result.Longitude;
    }

    private async Task SaveHouseholdAsync()
    {
        if (this.m_SavingHousehold)
            return;

        this.m_SavingHousehold = true;
        this.m_HouseholdSaved = false;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateHouseholdSettingsWebAppRequest, bool>(
            new() { Name = new(this.m_Name.Trim()) },
            ApiProvider.UpdateHouseholdSettings(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingHousehold = false;
        this.m_HouseholdSaved = _Result == true;
    }

    private async Task SaveLocationAsync()
    {
        if (this.m_SavingLocation)
            return;

        this.m_SavingLocation = true;
        this.m_LocationSaved = false;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateHouseholdSettingsWebAppRequest, bool>(
            new()
            {
                Latitude = new(this.m_Latitude),
                Longitude = new(this.m_Longitude)
            },
            ApiProvider.UpdateHouseholdSettings(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingLocation = false;
        this.m_LocationSaved = _Result == true;
    }

    private async Task SaveLifxTokenAsync()
    {
        if (this.m_SavingConnection)
            return;

        this.m_SavingConnection = true;
        this.m_ConnectionSaved = false;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateHouseholdSettingsWebAppRequest, bool>(
            new() { LifxApiToken = new(this.m_LifxToken.Trim()) },
            ApiProvider.UpdateHouseholdSettings(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingConnection = false;

        if (_Result != true)
            return;

        this.m_ConnectionSaved = true;
        this.m_LifxToken = string.Empty;

        await this.LoadSettingsAsync();
    }

    // An empty token disconnects — the server clears it.
    private async Task DisconnectLifxAsync()
    {
        if (this.m_SavingConnection)
            return;

        this.m_SavingConnection = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateHouseholdSettingsWebAppRequest, bool>(
            new() { LifxApiToken = new(string.Empty) },
            ApiProvider.UpdateHouseholdSettings(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingConnection = false;

        if (_Result == true)
            await this.LoadSettingsAsync();
    }

    private void SignOut()
        => this.NavigationManager.NavigateTo("/logout", true);

    #endregion Methods

}
