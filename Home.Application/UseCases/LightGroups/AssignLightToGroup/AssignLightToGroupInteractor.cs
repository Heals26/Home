using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightGroups.AssignLightToGroup;

/// <summary>
/// Moves a bulb into one of Home's groups. This is what makes Home's grouping authoritative — a
/// later sync will not undo it.
/// </summary>
internal class AssignLightToGroupInteractor : IInteractor<AssignLightToGroupInputPort, IAssignLightToGroupOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AssignLightToGroupInputPort inputPort,
        IAssignLightToGroupOutputPort outputPort,
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

        var _Light = _PersistenceContext.GetEntities<Light>()
            .Where(l => l.ID == inputPort.LightID
                && l.Group.Location.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Light == null)
        {
            await outputPort.PresentLightNotFoundAsync(inputPort.LightID, cancellationToken);
            return;
        }

        _Light.Group = _Group;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightAssignedAsync(cancellationToken);
    }

    #endregion Methods

}
