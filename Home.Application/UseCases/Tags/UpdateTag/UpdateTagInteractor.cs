using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Tags.UpdateTag;

internal class UpdateTagInteractor : IInteractor<UpdateTagInputPort, IUpdateTagOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateTagInputPort inputPort,
        IUpdateTagOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Tag = _PersistenceContext.GetEntities<Tag>()
            .Where(t => t.TagID == inputPort.TagID
                && t.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Tag == null)
        {
            await outputPort.PresentTagNotFoundAsync(inputPort.TagID, cancellationToken);
            return;
        }

        if (inputPort.Colour.HasBeenSet)
            _Tag.Colour = inputPort.Colour.Value.ToUpperInvariant();

        if (inputPort.Name.HasBeenSet)
        {
            var _Name = inputPort.Name.Value.Trim();

            var _IsNameTaken = _PersistenceContext.GetEntities<Tag>()
                .Any(t => t.Name == _Name
                    && t.TagID != _Tag.TagID
                    && t.Household.HouseholdID == _Household.HouseholdID);

            if (_IsNameTaken)
            {
                _ = await outputPort.PresentTagNameTakenAsync(_Name, cancellationToken);
                return;
            }

            _Tag.Name = _Name;
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentTagUpdatedAsync(cancellationToken);
    }

    #endregion Methods

}
