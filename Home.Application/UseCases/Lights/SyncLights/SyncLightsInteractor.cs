using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Lights.SyncLights;

/// <summary>
/// Pulls the bulb list from the provider and reconciles it into Home's own records, so the Lights
/// page can be served from the database instead of a round trip per page load.
/// </summary>
internal class SyncLightsInteractor : IInteractor<SyncLightsInputPort, ISyncLightsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SyncLightsInputPort inputPort,
        ISyncLightsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _LightService = serviceFactory.GetService<ILightService>();

        var _Snapshots = await _LightService.GetLightsAsync(cancellationToken);

        if (_Snapshots == null)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        var _Household = _AuthorisationService.GetHousehold();
        var _Now = DateTime.UtcNow;

        // Projected rather than Include()d, matching how the rest of the app forces eager loading.
        var _Locations = _PersistenceContext.GetEntities<LightLocation>()
            .Where(l => l.Household.HouseholdID == _Household.HouseholdID)
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

            var _Group = ResolveGroup(_PersistenceContext, _Locations, _Household, _Snapshot);

            _Light = new Light() { ID = _Snapshot.ID, Name = _Snapshot.Label, Group = _Group };
            ApplyState(_Light, _Snapshot, _Now);

            _Group.Lights.Add(_Light);
            _PersistenceContext.Add(_Light);
            _Added++;
        }

        // A bulb that has left the account cannot be controlled, so it should not be listed.
        var _LiveIDs = _Snapshots.Select(s => s.ID).ToHashSet();
        var _Removed = _ExistingLights.Values.Where(l => !_LiveIDs.Contains(l.ID)).ToList();

        _PersistenceContext.RemoveRange(_Removed);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightsSyncedAsync(_Added, _Updated, _Removed.Count, cancellationToken);
    }

    private static void ApplyState(Light light, LightSnapshot snapshot, DateTime nowUTC)
    {
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
    private static LightGroup ResolveGroup(
        IPersistenceContext persistenceContext,
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
