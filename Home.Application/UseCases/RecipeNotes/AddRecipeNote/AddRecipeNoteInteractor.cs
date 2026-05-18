using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeNotes.AddRecipeNote;

internal class AddRecipeNoteInteractor : IInteractor<AddRecipeNoteInputPort, IAddRecipeNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddRecipeNoteInputPort inputPort,
        IAddRecipeNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID)
            .Select(r => new { Recipe = r, r.Notes })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Note = new Note()
        {
            Content = inputPort.Content,
            CreatedOnUTC = DateTime.UtcNow
        };

        _PersistenceContext.Add(_Note);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        _Recipe.Notes.Add(new RecipeNote()
        {
            NoteID = _Note.NoteID,
            RecipeID = _Recipe.RecipeID
        });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeNoteAddedAsync(_Note.NoteID, cancellationToken);
    }

    #endregion Methods

}
