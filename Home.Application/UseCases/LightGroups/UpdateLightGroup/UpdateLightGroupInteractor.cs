using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightGroups.UpdateLightGroup;

internal class UpdateLightGroupInteractor : IInteractor<UpdateLightGroupInputPort, IUpdateLightGroupOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateLightGroupInputPort inputPort,
        IUpdateLightGroupOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Group = _PersistenceContext.GetEntities<LightGroup>()
            .Where(g => g.LightGroupID == inputPort.LightGroupID
                && g.Location.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Group == null)
        {
            await outputPort.PresentLightGroupNotFoundAsync(inputPort.LightGroupID, cancellationToken);
            return;
        }

        if (inputPort.Name.HasBeenSet)
            _Group.Name = inputPort.Name.Value.Trim();

        if (inputPort.Sequence.HasBeenSet)
            _Group.Sequence = inputPort.Sequence.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightGroupUpdatedAsync(cancellationToken);
    }

    #endregion Methods

}
