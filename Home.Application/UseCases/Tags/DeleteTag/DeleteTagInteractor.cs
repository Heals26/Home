using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Tags.DeleteTag;

internal class DeleteTagInteractor : IInteractor<DeleteTagInputPort, IDeleteTagOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteTagInputPort inputPort,
        IDeleteTagOutputPort outputPort,
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

        // The join has no cascade on the tag side, so the rows go first or the delete is rejected.
        var _ActivityTags = _PersistenceContext.GetEntities<ActivityTag>()
            .Where(t => t.TagID == _Tag.TagID)
            .ToList();

        _PersistenceContext.RemoveRange(_ActivityTags);
        _PersistenceContext.Remove(_Tag);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentTagDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
