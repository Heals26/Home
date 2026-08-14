using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;

namespace Home.Application.UseCases.Lights.SyncLights;

/// <summary>
/// Pulls the bulb list from the provider and reconciles it into Home's own records, so the Lights
/// page can be served from the database instead of a round trip per page load. The reconcile
/// itself lives in <c>ILightSyncLogic</c>, shared with the background sync runner.
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
        var _LightSyncLogic = serviceFactory.GetService<ILightSyncLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Result = await _LightSyncLogic.SyncHouseholdAsync(_Household, cancellationToken);

        if (_Result == null)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightsSyncedAsync(_Result.Added, _Result.Updated, _Result.Removed, cancellationToken);
    }

    #endregion Methods

}
