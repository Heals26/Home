namespace Home.Application.UseCases.RecipeNotes.AddRecipeNote;

public interface IAddRecipeNoteOutputPort
{

    #region Methods

    Task PresentRecipeNoteAddedAsync(long noteID, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
