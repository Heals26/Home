using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.ApplyLightScene;

internal class ApplyLightSceneInteractor
    : IInteractor<ApplyLightSceneInputPort, IApplyLightSceneOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        ApplyLightSceneInputPort inputPort,
        IApplyLightSceneOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _SceneLogic = serviceFactory.GetService<ILightSceneLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Scene = _PersistenceContext.GetEntities<LightScene>()
            .Where(s => s.LightSceneID == inputPort.LightSceneID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new { Scene = s, States = s.States.Select(st => new { State = st, st.Light }) })
            .SingleOrDefault()
            ?.Scene;

        if (_Scene == null)
        {
            await outputPort.PresentLightSceneNotFoundAsync(inputPort.LightSceneID, cancellationToken);
            return;
        }

        var _Result = await _SceneLogic.ApplyAsync(_Scene, cancellationToken);

        if (_Result == LightCommandResult.Unavailable)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightSceneAppliedAsync(cancellationToken);
    }

    #endregion Methods

}
