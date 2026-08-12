using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Lights.GetLights;

/// <summary>
/// Served entirely from Home's own records — opening the Lights page costs no provider calls.
/// State is whatever the last sync or command wrote; SyncLights refreshes it.
/// </summary>
internal class GetLightsInteractor : IInteractor<GetLightsInputPort, IGetLightsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetLightsInputPort inputPort,
        IGetLightsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Groups = _PersistenceContext.GetEntities<LightGroup>()
            .Where(g => g.Location.Household.HouseholdID == _Household.HouseholdID)
            .Select(g => new { Group = g, g.Lights })
            .ToList()
            .Select(g => g.Group)
            .OrderBy(g => g.Sequence)
            .ThenBy(g => g.Name)
            .ToList();

        await outputPort.PresentLightsAsync(_Groups, cancellationToken);
    }

    #endregion Methods

}
