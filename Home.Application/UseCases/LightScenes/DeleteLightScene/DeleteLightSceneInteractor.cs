using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.DeleteLightScene;

internal class DeleteLightSceneInteractor : IInteractor<DeleteLightSceneInputPort, IDeleteLightSceneOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteLightSceneInputPort inputPort,
        IDeleteLightSceneOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Scene = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.LightSceneID == inputPort.LightSceneID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Scene == null)
        {
            await outputPort.PresentLightSceneNotFoundAsync(inputPort.LightSceneID, cancellationToken);
            return;
        }

        // The saved states cascade with it; a scene is nothing without them.
        _PersistenceContext.Remove(_Scene);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightSceneDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
