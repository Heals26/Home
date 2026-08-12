using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightGroups.CreateLightGroup;

internal class CreateLightGroupInteractor : IInteractor<CreateLightGroupInputPort, ICreateLightGroupOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateLightGroupInputPort inputPort,
        ICreateLightGroupOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Locations = _PersistenceContext.GetEntities<LightLocation>()
            .Where(l => l.Household.HouseholdID == _Household.HouseholdID)
            .Select(l => new { Location = l, Groups = l.Groups.Select(g => g.Sequence) })
            .ToList();

        // Groups hang off a location, and locations only exist once lights have been synced.
        // Without one there is nothing to attach the group to.
        var _Location = _Locations.FirstOrDefault();

        if (_Location == null)
        {
            await outputPort.PresentNoLocationAsync(cancellationToken);
            return;
        }

        var _Group = new LightGroup()
        {
            Name = inputPort.Name.Trim(),
            Location = _Location.Location,
            Sequence = _Location.Groups.Any() ? _Location.Groups.Max() + 1 : 0,
            Lights = []
        };

        _PersistenceContext.Add(_Group);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightGroupCreatedAsync(_Group.LightGroupID, cancellationToken);
    }

    #endregion Methods

}
