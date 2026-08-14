using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Notes.UpdateNote;

internal class UpdateNoteInteractor : IInteractor<UpdateNoteInputPort, IUpdateNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateNoteInputPort inputPort,
        IUpdateNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _NoteIsInHousehold = _PersistenceContext.GetEntities<RecipeNote>()
                .Any(rn => rn.NoteID == inputPort.NoteID && rn.Recipe.Household.HouseholdID == _Household.HouseholdID)
            || _PersistenceContext.GetEntities<IngredientNote>()
                .Any(n => n.NoteID == inputPort.NoteID
                    && n.Ingredient.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID));

        var _Note = _NoteIsInHousehold
            ? _PersistenceContext.Find<Note>(inputPort.NoteID)
            : null;

        if (_Note == null)
        {
            await outputPort.PresentNoteNotFoundAsync(inputPort.NoteID, cancellationToken);
            return;
        }

        if (inputPort.Content.HasBeenSet)
            _Note.Content = inputPort.Content.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentNoteNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
