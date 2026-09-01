using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Households.GetHouseholdSettings;
using Home.WebUI.DataAccess.Households.UpdateHouseholdSettings;
using Home.WebUI.DataAccess.Users.CreateUser;
using Home.WebUI.DataAccess.Users.GetUsers;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.DataAccess.Users.UpdateUser;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Security;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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

    // Editing a member
    private bool m_ShowEditMember;
    private long? m_EditingUserID;
    private bool m_EditingSelf;
    private bool m_SavingMember;
    private bool m_RemovingMember;

    /// <summary>
    /// Who is signed in on this device, read once from the cookie's claims. Null when the claim
    /// cannot be read, which only costs the "You" badge and the guard against self-removal — both
    /// fail closed, so an unknown member is never treated as this one.
    /// </summary>
    private long? m_SignedInUserID;

    // Changing your own password
    private bool m_ShowChangePassword;
    private string m_NewPassword = string.Empty;
    private string m_ConfirmPassword = string.Empty;
    private bool m_ChangingPassword;

    #endregion Fields

    #region Properties

    [CascadingParameter] public Task<AuthenticationState>? AuthenticationState { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        if (this.AuthenticationState != null)
            this.m_SignedInUserID = HouseholdClaims.GetUserID((await this.AuthenticationState).User);

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

    private bool IsSignedInMember(UserSummaryDto user)
        => this.m_SignedInUserID != null && user.UserID == this.m_SignedInUserID;

    private void OpenEditMemberModal(UserSummaryDto user)
    {
        this.m_EditingUserID = user.UserID;
        this.m_EditingSelf = this.IsSignedInMember(user);
        this.m_MemberFirstName = user.FirstName;
        this.m_MemberLastName = user.LastName;
        this.m_MemberEmail = user.Email;
        this.m_ShowEditMember = true;
    }

    private bool CanSaveMember()
        => !string.IsNullOrWhiteSpace(this.m_MemberFirstName)
            && !string.IsNullOrWhiteSpace(this.m_MemberLastName)
            && !string.IsNullOrWhiteSpace(this.m_MemberEmail);

    /// <summary>
    /// The password tracker is deliberately left unset — this form does not carry one, and a
    /// tracker that arrived "set" to empty would blank the member's password.
    /// </summary>
    private async Task SaveMemberAsync()
    {
        if (this.m_SavingMember || !this.m_EditingUserID.HasValue || !this.CanSaveMember())
            return;

        this.m_SavingMember = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateUserWebAppRequest, bool>(
            new UpdateUserWebAppRequest()
            {
                Email = new(this.m_MemberEmail.Trim()),
                FirstName = new(this.m_MemberFirstName.Trim()),
                LastName = new(this.m_MemberLastName.Trim())
            },
            ApiProvider.UpdateUser(this.m_EditingUserID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingMember = false;

        if (_Result != true)
            return;

        this.m_ShowEditMember = false;

        await this.LoadUsersAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Users, this.m_CancellationTokenHandler.Token);
    }

    /// <summary>
    /// History outlives the person: <c>Audit → User</c> is SetNull and <c>Audit.UserName</c> is
    /// denormalised onto the row, so removing a member keeps what they did (15 Aug).
    /// </summary>
    private async Task RemoveMemberAsync()
    {
        if (this.m_RemovingMember || !this.m_EditingUserID.HasValue || this.m_EditingSelf)
            return;

        this.m_RemovingMember = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteUser(this.m_EditingUserID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_RemovingMember = false;

        if (_Result != true)
            return;

        this.m_ShowEditMember = false;

        await this.LoadUsersAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Users, this.m_CancellationTokenHandler.Token);
    }

    private void OpenChangePasswordModal()
    {
        this.m_NewPassword = string.Empty;
        this.m_ConfirmPassword = string.Empty;
        this.m_ShowChangePassword = true;
    }

    /// <summary>
    /// Said out loud rather than left to a disabled button with no explanation — a control that
    /// does nothing and says nothing is the frustration this product exists to avoid.
    /// <para>
    /// Only the confirmation is checked. There is deliberately no length or complexity rule here:
    /// the API asks for a non-empty password and nothing more, and a rule invented on this side
    /// would reject passwords the household already signs in with.
    /// </para>
    /// </summary>
    private string PasswordProblem()
        => this.m_ConfirmPassword.Length > 0 && this.m_ConfirmPassword != this.m_NewPassword
            ? "Those two don't match."
            : string.Empty;

    private async Task ChangePasswordAsync()
    {
        if (this.m_ChangingPassword || this.m_SignedInUserID == null || this.PasswordProblem().Length > 0 || this.m_NewPassword.Length == 0)
            return;

        this.m_ChangingPassword = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateUserWebAppRequest, bool>(
            new UpdateUserWebAppRequest() { Password = new(this.m_NewPassword) },
            ApiProvider.UpdateUser(this.m_SignedInUserID.Value),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_ChangingPassword = false;

        if (_Result != true)
            return;

        this.m_NewPassword = string.Empty;
        this.m_ConfirmPassword = string.Empty;
        this.m_ShowChangePassword = false;
    }

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
