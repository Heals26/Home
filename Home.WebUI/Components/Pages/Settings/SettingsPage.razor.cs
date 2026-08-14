using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Households.GetHouseholdSettings;
using Home.WebUI.DataAccess.Households.UpdateHouseholdSettings;
using Home.WebUI.DataAccess.Users.CreateUser;
using Home.WebUI.DataAccess.Users.GetUsers;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.Settings;

public partial class SettingsPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
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

    // Members
    private ICollection<UserSummaryDto>? m_Users;
    private bool m_ShowAddMember;
    private string m_MemberFirstName = string.Empty;
    private string m_MemberLastName = string.Empty;
    private string m_MemberEmail = string.Empty;
    private string m_MemberPassword = string.Empty;
    private bool m_AddingMember;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(this.LoadSettingsAsync(), this.LoadUsersAsync());

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

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

        if (_Result == true)
            await this.ChangeBroadcaster.PublishAsync(ChangeArea.Settings, this.m_CancellationTokenHandler.Token);
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

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.Users && area != ChangeArea.Settings)
            return;

        await this.InvokeAsync(async () =>
        {
            await (area == ChangeArea.Users ? this.LoadUsersAsync() : this.LoadSettingsAsync());
            this.StateHasChanged();
        });
    }

    private async Task LoadUsersAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetUsersWebAppResponse>(
            null!, ApiProvider.GetUsers(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Users = _Result.Users;
    }

    private void OpenAddMemberModal()
    {
        this.m_MemberFirstName = string.Empty;
        this.m_MemberLastName = string.Empty;
        this.m_MemberEmail = string.Empty;
        this.m_MemberPassword = string.Empty;
        this.m_ShowAddMember = true;
    }

    private async Task AddMemberAsync()
    {
        if (this.m_AddingMember)
            return;

        this.m_AddingMember = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateUserWebAppRequest, CreateUserWebAppResponse>(
            new CreateUserWebAppRequest()
            {
                Email = this.m_MemberEmail.Trim(),
                FirstName = this.m_MemberFirstName.Trim(),
                LastName = this.m_MemberLastName.Trim(),
                Password = this.m_MemberPassword
            },
            ApiProvider.CreateUser(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_AddingMember = false;

        if (_Result == null)
            return;

        this.m_ShowAddMember = false;

        await this.LoadUsersAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Users, this.m_CancellationTokenHandler.Token);
    }

    private bool CanAddMember()
        => !string.IsNullOrWhiteSpace(this.m_MemberFirstName)
            && !string.IsNullOrWhiteSpace(this.m_MemberLastName)
            && !string.IsNullOrWhiteSpace(this.m_MemberEmail)
            && !string.IsNullOrWhiteSpace(this.m_MemberPassword);

    private static string Initials(string name)
    {
        var _Parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _Parts.Length switch
        {
            0 => "?",
            1 => _Parts[0][..1].ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(_Parts[0][0])}{char.ToUpperInvariant(_Parts[^1][0])}"
        };
    }

    private void SignOut()
        => this.NavigationManager.NavigateTo("/logout", true);

    #endregion Methods

}
