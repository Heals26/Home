using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Devices.GetDevices;
using Home.WebUI.DataAccess.Devices.SignOutOtherDevices;
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
using System.Globalization;

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
    private string m_Latitude = string.Empty;
    private string m_Longitude = string.Empty;
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
    private bool m_MemberHasLogin = true;
    private bool m_EditingHasLogin;
    private bool m_AddingMember;

    // Devices
    private List<DeviceDto>? m_Devices;
    private long? m_SigningOutDeviceID;
    private bool m_ShowAllDevices;
    private bool m_SigningOutOthers;

    /// <summary>
    /// How many devices the card shows before it stops. A household accumulates a session per
    /// sign-in, so the full list can run to dozens and a wall of rows is not a screen anybody
    /// reads. Enough to cover the devices a family actually uses.
    /// </summary>
    private const int VisibleDeviceCount = 5;

    // Editing a member
    private bool m_ShowEditMember;
    private long? m_EditingUserID;
    private bool m_EditingSelf;
    private bool m_SavingMember;
    private bool m_RemovingMember;

    /// <summary>
    /// Who is signed in on this device, read once from the cookie's claims. Null when the claim
    /// cannot be read, which only costs the "You" badge and the guard against self-removal. Both
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

        await Task.WhenAll(this.LoadSettingsAsync(), this.LoadUsersAsync(), this.LoadDevicesAsync());

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
        this.m_Latitude = _Result.Latitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        this.m_Longitude = _Result.Longitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
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
                Latitude = new(ParseCoordinate(this.m_Latitude)),
                Longitude = new(ParseCoordinate(this.m_Longitude))
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

    // An empty token disconnects, because the server clears it.
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

    private async Task LoadDevicesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetDevicesWebAppResponse>(
            null!, ApiProvider.GetDevices(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Devices = _Result.Devices;
    }

    /// <summary>
    /// The rows to draw, which is all of them once the family has asked to see them.
    /// </summary>
    private IEnumerable<DeviceDto> VisibleDevices()
        => this.m_ShowAllDevices || this.m_Devices == null
            ? this.m_Devices ?? []
            : this.m_Devices.Take(VisibleDeviceCount);

    private int HiddenDeviceCount()
        => Math.Max(0, (this.m_Devices?.Count ?? 0) - VisibleDeviceCount);

    private int OtherDeviceCount()
        => this.m_Devices?.Count(d => !d.IsCurrentDevice) ?? 0;

    /// <summary>
    /// Named for what it does to how many, so the button says the same thing the confirmation will.
    /// </summary>
    private string SignOutOthersLabel()
        => this.OtherDeviceCount() == 1
            ? "Sign out 1 other"
            : $"Sign out {this.OtherDeviceCount()} others";

    private async Task SignOutOtherDevicesAsync()
    {
        if (this.m_SigningOutOthers)
            return;

        this.m_SigningOutOthers = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, SignOutOtherDevicesWebAppResponse>(
            null!, ApiProvider.SignOutOtherDevices(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SigningOutOthers = false;

        if (_Result == null)
            return;

        this.m_ShowAllDevices = false;

        await this.LoadDevicesAsync();
    }

    private async Task SignOutDeviceAsync(DeviceDto device)
    {
        if (this.m_SigningOutDeviceID != null)
            return;

        this.m_SigningOutDeviceID = device.AuthenticationMetadataID;

        _ = await this.ApiAccess.SendRequestAsync<object, object>(
            null!, ApiProvider.SignOutDevice(device.AuthenticationMetadataID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SigningOutDeviceID = null;

        await this.LoadDevicesAsync();
    }

    /// <summary>
    /// How long ago something happened, at the coarsest useful grain. A list of devices answers
    /// "is this one still in use", which minutes and seconds do not help with.
    /// </summary>
    private string DescribeWhen(DateTime? whenUTC)
    {
        if (whenUTC == null)
            return "Not used since signing in";

        var _Elapsed = this.TimeProvider.GetUtcNow().UtcDateTime - whenUTC.Value;

        return _Elapsed switch
        {
            { TotalMinutes: < 2 } => "Active now",
            { TotalMinutes: < 60 } => $"{(int)_Elapsed.TotalMinutes} minutes ago",
            { TotalHours: < 2 } => "An hour ago",
            { TotalHours: < 24 } => $"{(int)_Elapsed.TotalHours} hours ago",
            { TotalDays: < 2 } => "Yesterday",
            { TotalDays: < 31 } => $"{(int)_Elapsed.TotalDays} days ago",
            _ => whenUTC.Value.ToLocalTime().ToString("d MMM yyyy")
        };
    }

    private void OpenAddMemberModal()
    {
        this.m_MemberFirstName = string.Empty;
        this.m_MemberLastName = string.Empty;
        this.m_MemberEmail = string.Empty;
        this.m_MemberPassword = string.Empty;
        this.m_MemberHasLogin = true;
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
                Email = this.m_MemberHasLogin ? this.m_MemberEmail.Trim() : string.Empty,
                FirstName = this.m_MemberFirstName.Trim(),
                LastName = this.m_MemberLastName.Trim(),
                Password = this.m_MemberHasLogin ? this.m_MemberPassword : string.Empty
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
            && (!this.m_MemberHasLogin
                || (!string.IsNullOrWhiteSpace(this.m_MemberEmail) && !string.IsNullOrWhiteSpace(this.m_MemberPassword)));

    private bool IsSignedInMember(UserSummaryDto user)
        => this.m_SignedInUserID != null && user.UserID == this.m_SignedInUserID;

    private void OpenEditMemberModal(UserSummaryDto user)
    {
        this.m_EditingUserID = user.UserID;
        this.m_EditingSelf = this.IsSignedInMember(user);
        this.m_MemberFirstName = user.FirstName;
        this.m_MemberLastName = user.LastName;
        this.m_MemberEmail = user.Email;
        this.m_MemberPassword = string.Empty;
        this.m_EditingHasLogin = user.HasLogin;
        this.m_MemberHasLogin = user.HasLogin;
        this.m_ShowEditMember = true;
    }

    /// <summary>
    /// A member who already signs in keeps needing an email; one being given a sign-in needs both
    /// halves of it; one staying without needs only a name.
    /// </summary>
    private bool CanSaveMember()
        => !string.IsNullOrWhiteSpace(this.m_MemberFirstName)
            && !string.IsNullOrWhiteSpace(this.m_MemberLastName)
            && (this.m_EditingHasLogin
                ? !string.IsNullOrWhiteSpace(this.m_MemberEmail)
                : !this.m_MemberHasLogin || (!string.IsNullOrWhiteSpace(this.m_MemberEmail) && !string.IsNullOrWhiteSpace(this.m_MemberPassword)));

    /// <summary>
    /// The password tracker is only set when a sign-in is being added, because a tracker that
    /// arrived "set" to empty would blank an existing member's password. The email tracker is only
    /// sent when there is one to send, so a member without a sign-in stays without one.
    /// </summary>
    private async Task SaveMemberAsync()
    {
        if (this.m_SavingMember || !this.m_EditingUserID.HasValue || !this.CanSaveMember())
            return;

        this.m_SavingMember = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateUserWebAppRequest, bool>(
            new UpdateUserWebAppRequest()
            {
                Email = this.m_EditingHasLogin || this.m_MemberHasLogin ? new(this.m_MemberEmail.Trim()) : default,
                FirstName = new(this.m_MemberFirstName.Trim()),
                LastName = new(this.m_MemberLastName.Trim()),
                Password = !this.m_EditingHasLogin && this.m_MemberHasLogin ? new(this.m_MemberPassword) : default
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
    /// Said out loud rather than left to a disabled button with no explanation. A control that
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

    /// <summary>
    /// Invariant, because a device set to a locale that writes -33,86 would otherwise send a
    /// latitude the API reads as a different place.
    /// </summary>
    private static double? ParseCoordinate(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var _Parsed)
            ? _Parsed
            : null;

    #endregion Methods

}
