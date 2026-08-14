using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;

internal class RemoveRecipeNoteInteractor : IInteractor<RemoveRecipeNoteInputPort, IRemoveRecipeNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RemoveRecipeNoteInputPort inputPort,
        IRemoveRecipeNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Link = _PersistenceContext.GetEntities<RecipeNote>()
            .Where(rn => rn.RecipeID == inputPort.RecipeID && rn.NoteID == inputPort.NoteID
                && rn.Recipe.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Link == null)
        {
            await outputPort.PresentRecipeNoteNotFoundAsync(inputPort.NoteID, cancellationToken);
            return;
        }

        var _Note = _PersistenceContext.Find<Note>(inputPort.NoteID);

        _PersistenceContext.Remove(_Link);
        if (_Note != null)
            _PersistenceContext.Remove(_Note);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeNoteRemovedAsync(cancellationToken);
    }

    #endregion Methods

}
