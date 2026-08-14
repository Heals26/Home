using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.IngredientNotes.AddIngredientNote;

internal class AddIngredientNoteInteractor : IInteractor<AddIngredientNoteInputPort, IAddIngredientNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddIngredientNoteInputPort inputPort,
        IAddIngredientNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Ingredient = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => i.IngredientID == inputPort.IngredientID
                && i.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID))
            .Select(i => new
            {
                Ingredient = i,
                i.Notes
            })
            .SingleOrDefault()
            ?.Ingredient;

        if (_Ingredient == null)
        {
            await outputPort.PresentIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
            return;
        }

        var _Note = new Note()
        {
            Content = inputPort.Content,
            CreatedOnUTC = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime
        };

        _PersistenceContext.Add(_Note);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        _Ingredient.Notes.Add(new IngredientNote()
        {
            NoteID = _Note.NoteID,
            IngredientID = _Ingredient.IngredientID
        });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentIngredientNoteAddedAsync(_Note.NoteID, cancellationToken);
    }

    #endregion Methods

}
