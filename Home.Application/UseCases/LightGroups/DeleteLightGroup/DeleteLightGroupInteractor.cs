using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightGroups.DeleteLightGroup;

internal class DeleteLightGroupInteractor : IInteractor<DeleteLightGroupInputPort, IDeleteLightGroupOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteLightGroupInputPort inputPort,
        IDeleteLightGroupOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Group = _PersistenceContext.GetEntities<LightGroup>()
            .Where(g => g.LightGroupID == inputPort.LightGroupID
                && g.Location.Household.HouseholdID == _Household.HouseholdID)
            .Select(g => new { Group = g, g.Lights })
            .SingleOrDefault()
            ?.Group;

        if (_Group == null)
        {
            await outputPort.PresentLightGroupNotFoundAsync(inputPort.LightGroupID, cancellationToken);
            return;
        }

        // The FK cascades, so deleting a populated group would silently delete the bulbs with it
        // and they would only come back on the next sync. Make the caller move them first.
        if (_Group.Lights.Count > 0)
        {
            await outputPort.PresentLightGroupNotEmptyAsync(
                inputPort.LightGroupID, _Group.Lights.Count, cancellationToken);

            return;
        }

        _PersistenceContext.Remove(_Group);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightGroupDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
