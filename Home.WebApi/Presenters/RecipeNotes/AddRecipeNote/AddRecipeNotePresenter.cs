using AutoMapper;
using Home.Application.UseCases.RecipeNotes.AddRecipeNote;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.RecipeNotes.AddRecipeNote;

namespace Home.WebApi.Presenters.RecipeNotes.AddRecipeNote;

public class AddRecipeNotePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddRecipeNoteOutputPort
{

    #region Methods

    Task IAddRecipeNoteOutputPort.PresentRecipeNoteAddedAsync(long noteID, CancellationToken cancellationToken)
        => this.CreatedAsync(noteID, new AddRecipeNoteApiResponse() { NoteID = noteID }, cancellationToken);

    Task IAddRecipeNoteOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
