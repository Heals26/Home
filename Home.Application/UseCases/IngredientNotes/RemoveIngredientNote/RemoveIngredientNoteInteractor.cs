using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;

internal class RemoveIngredientNoteInteractor : IInteractor<RemoveIngredientNoteInputPort, IRemoveIngredientNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RemoveIngredientNoteInputPort inputPort,
        IRemoveIngredientNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Link = _PersistenceContext.GetEntities<IngredientNote>()
            .Where(n => n.IngredientID == inputPort.IngredientID && n.NoteID == inputPort.NoteID
                && n.Ingredient.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID))
            .SingleOrDefault();

        if (_Link == null)
        {
            await outputPort.PresentIngredientNoteNotFoundAsync(inputPort.NoteID, cancellationToken);
            return;
        }

        var _Note = _PersistenceContext.Find<Note>(inputPort.NoteID);

        _PersistenceContext.Remove(_Link);
        if (_Note != null)
            _PersistenceContext.Remove(_Note);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentIngredientNoteRemovedAsync(cancellationToken);
    }

    #endregion Methods

}
