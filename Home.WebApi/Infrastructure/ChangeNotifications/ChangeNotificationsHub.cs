#nullable enable
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Home.WebApi.Infrastructure.ChangeNotifications;

/// <summary>
/// Relays "something changed" between a household's devices. The group is always derived from
/// the caller's authenticated claims — a client can neither choose nor spoof a household, so
/// one family's changes can never reach another family's devices.
/// </summary>
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ChangeNotificationsHub(IPersistenceContext persistenceContext) : Hub
{

    #region Methods

    public override async Task OnConnectedAsync()
    {
        var _HouseholdID = this.ResolveHouseholdID();

        if (_HouseholdID != null)
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, GroupName(_HouseholdID.Value));

        await base.OnConnectedAsync();
    }

    public async Task PublishAsync(string area)
    {
        var _HouseholdID = this.ResolveHouseholdID();

        if (_HouseholdID != null)
            await this.Clients.Group(GroupName(_HouseholdID.Value)).SendAsync("Changed", area);
    }

    internal static string GroupName(long householdID)
        => $"household-{householdID}";

    private long? ResolveHouseholdID()
    {
        var _UserIDValue = this.Context.User?.FindFirst(nameof(AuthenticationMetadata.UserID))?.Value;

        if (!long.TryParse(_UserIDValue, out var _UserID))
            return null;

        return persistenceContext.GetEntities<Household>()
            .Where(h => h.Members.Any(m => m.UserID == _UserID))
            .Select(h => (long?)h.HouseholdID)
            .SingleOrDefault();
    }

    #endregion Methods

}
