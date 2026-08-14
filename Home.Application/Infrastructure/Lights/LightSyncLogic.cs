using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Lights;

/// <summary>
/// The bulb-list reconcile shared by the Sync button and the background sync runner, so a
/// physically switched-off light shows up without anyone pressing anything.
/// </summary>
public class LightSyncLogic(
    ILightService lightService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : ILightSyncLogic
{

    #region Methods

    async Task<LightSyncResult?> ILightSyncLogic.SyncHouseholdAsync(Household household, CancellationToken cancellationToken)
    {
        var _Snapshots = await lightService.GetLightsAsync(cancellationToken);

        if (_Snapshots == null)
            return null;

        var _Now = timeProvider.GetUtcNow().UtcDateTime;

        // Projected rather than Include()d, matching how the rest of the app forces eager loading.
        var _Locations = persistenceContext.GetEntities<LightLocation>()
            .Where(l => l.Household.HouseholdID == household.HouseholdID)
            .Select(l => new
            {
                Location = l,
                Groups = l.Groups.Select(g => new { Group = g, g.Lights })
            })
            .ToList()
            .Select(l => l.Location)
            .ToList();

        var _ExistingLights = _Locations
            .SelectMany(l => l.Groups)
            .SelectMany(g => g.Lights)
            .ToDictionary(l => l.ID);

        var _Added = 0;
        var _Updated = 0;

        foreach (var _Snapshot in _Snapshots)
        {
            if (_ExistingLights.TryGetValue(_Snapshot.ID, out var _Light))
            {
                // Name and state refresh, but never the group: once a bulb is in Home it belongs
                // to whichever group the user put it in, not whichever group LIFX still thinks.
                _Light.Name = _Snapshot.Label;
                ApplyState(_Light, _Snapshot, _Now);
                _Updated++;
                continue;
            }

            var _Group = this.ResolveGroup(_Locations, household, _Snapshot);

            _Light = new Light() { ID = _Snapshot.ID, Name = _Snapshot.Label, Group = _Group };
            ApplyState(_Light, _Snapshot, _Now);

            _Group.Lights.Add(_Light);
            persistenceContext.Add(_Light);
            _Added++;
        }

        // A bulb that has left the account cannot be controlled, so it should not be listed.
        var _LiveIDs = _Snapshots.Select(s => s.ID).ToHashSet();
        var _Removed = _ExistingLights.Values.Where(l => !_LiveIDs.Contains(l.ID)).ToList();

        if (_Removed.Count > 0)
        {
            // LightSceneState deliberately does not cascade from Light — SQL Server rejects the
            // second cascade path from Household — so its rows are cleared here instead. Skipping
            // this would fail the delete on a foreign key.
            var _RemovedKeys = _Removed.Select(l => l.LightID).ToHashSet();

            var _OrphanedStates = persistenceContext.GetEntities<LightSceneState>()
                .Where(s => _RemovedKeys.Contains(s.Light.LightID))
                .ToList();

            persistenceContext.RemoveRange(_OrphanedStates);
        }

        persistenceContext.RemoveRange(_Removed);

        return new LightSyncResult(_Added, _Updated, _Removed.Count);
    }

    private static void ApplyState(Light light, LightSnapshot snapshot, DateTime nowUTC)
    {
        // Capabilities are refreshed too — a firmware update can change what a bulb reports.
        light.HasColour = snapshot.Capabilities.HasColour;
        light.HasMatrix = snapshot.Capabilities.HasMatrix;
        light.HasMultizone = snapshot.Capabilities.HasMultizone;
        light.HasVariableColourTemp = snapshot.Capabilities.HasVariableColourTemp;
        light.MinKelvin = snapshot.Capabilities.MinKelvin;
        light.MaxKelvin = snapshot.Capabilities.MaxKelvin;
        light.ProductName = snapshot.Capabilities.ProductName;

        light.Brightness = snapshot.Brightness;
        light.Hue = snapshot.Hue;
        light.IsConnected = snapshot.IsConnected;
        light.IsOn = snapshot.IsOn;
        light.Kelvin = snapshot.Kelvin;
        light.Saturation = snapshot.Saturation;
        light.StateUpdatedUTC = nowUTC;
    }

    /// <summary>
    /// Finds, or seeds, the group a newly discovered bulb should land in. Seeding mirrors the
    /// provider's own grouping once — after that the user owns it.
    /// </summary>
    private LightGroup ResolveGroup(
        List<LightLocation> locations,
        Household household,
        LightSnapshot snapshot)
    {
        var _Location = locations.FirstOrDefault(l => l.ID == snapshot.LocationID);

        if (_Location == null)
        {
            _Location = new LightLocation()
            {
                ID = snapshot.LocationID,
                Name = snapshot.LocationName,
                Household = household,
                Groups = []
            };

            locations.Add(_Location);
            persistenceContext.Add(_Location);
        }

        var _Group = _Location.Groups.FirstOrDefault(g => g.ID == snapshot.GroupID);

        if (_Group == null)
        {
            _Group = new LightGroup()
            {
                ID = snapshot.GroupID,
                Name = snapshot.GroupName,
                Location = _Location,
                Sequence = _Location.Groups.Count,
                Lights = []
            };

            _Location.Groups.Add(_Group);
            persistenceContext.Add(_Group);
        }

        return _Group;
    }

    #endregion Methods

}
