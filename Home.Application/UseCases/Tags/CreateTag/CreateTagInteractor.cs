using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Tags.CreateTag;

internal class CreateTagInteractor : IInteractor<CreateTagInputPort, ICreateTagOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateTagInputPort inputPort,
        ICreateTagOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Name = inputPort.Name.Trim();

        // The household and name pair is unique in the database, so catch it here rather than
        // letting the insert fail.
        var _IsNameTaken = _PersistenceContext.GetEntities<Tag>()
            .Any(t => t.Name == _Name && t.Household.HouseholdID == _Household.HouseholdID);

        if (_IsNameTaken)
        {
            _ = await outputPort.PresentTagNameTakenAsync(_Name, cancellationToken);
            return;
        }

        var _Tag = new Tag()
        {
            Activities = [],
            Colour = inputPort.Colour.ToUpperInvariant(),
            Household = _Household,
            Name = _Name
        };

        _PersistenceContext.Add(_Tag);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentTagCreatedAsync(_Tag.TagID, cancellationToken);
    }

    #endregion Methods

}
