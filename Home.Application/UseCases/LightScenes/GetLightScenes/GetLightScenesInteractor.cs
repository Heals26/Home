using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.GetLightScenes;

internal class GetLightScenesInteractor : IInteractor<GetLightScenesInputPort, IGetLightScenesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetLightScenesInputPort inputPort,
        IGetLightScenesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // The previous look leads: it is the undo, and an undo you have to hunt for isn't one.
        var _Scenes = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new { Scene = s, s.States })
            .ToList()
            .Select(s => s.Scene)
            .OrderByDescending(s => s.IsPreviousLook)
            .ThenBy(s => s.Sequence)
            .ThenBy(s => s.Name)
            .ToList();

        await outputPort.PresentLightScenesAsync(_Scenes, cancellationToken);
    }

    #endregion Methods

}
