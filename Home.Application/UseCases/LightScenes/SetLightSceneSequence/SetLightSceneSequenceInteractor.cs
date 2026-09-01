using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.SetLightSceneSequence;

internal class SetLightSceneSequenceInteractor : IInteractor<SetLightSceneSequenceInputPort, ISetLightSceneSequenceOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetLightSceneSequenceInputPort inputPort,
        ISetLightSceneSequenceOutputPort outputPort,
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
        }
        else
        {
            _Scene.Sequence = inputPort.Sequence;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentLightSceneSequenceSetAsync(cancellationToken);
        }
    }

    #endregion Methods

}
