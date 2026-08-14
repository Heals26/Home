using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Lights.SyncAllLights;

/// <summary>
/// The background counterpart of SyncLights: refreshes bulb state for every household with a
/// stored provider token, so a physically switched light shows up on the board without anyone
/// pressing Sync. Deliberately does not use <c>IAuthorisationService</c> — there is no signed-in
/// user behind a timer tick. Like the schedule runner, it relies on the background token
/// resolution, which supports a single tokened household today.
/// </summary>
internal class SyncAllLightsInteractor : IInteractor<SyncAllLightsInputPort, ISyncAllLightsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SyncAllLightsInputPort inputPort,
        ISyncAllLightsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _LightSyncLogic = serviceFactory.GetService<ILightSyncLogic>();

        var _Households = _PersistenceContext.GetEntities<Household>()
            .Where(h => h.LifxApiToken != null && h.LifxApiToken != string.Empty)
            .ToList();

        var _SyncedHouseholdIDs = new List<long>();
        var _Unavailable = 0;

        foreach (var _Household in _Households)
        {
            var _Result = await _LightSyncLogic.SyncHouseholdAsync(_Household, cancellationToken);

            if (_Result == null)
            {
                _Unavailable++;
                continue;
            }

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
            _SyncedHouseholdIDs.Add(_Household.HouseholdID);
        }

        await outputPort.PresentAllLightsSyncedAsync(_SyncedHouseholdIDs, _Unavailable, cancellationToken);
    }

    #endregion Methods

}
