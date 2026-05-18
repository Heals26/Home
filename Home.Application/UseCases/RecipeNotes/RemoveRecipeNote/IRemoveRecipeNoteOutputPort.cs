namespace Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;

public interface IRemoveRecipeNoteOutputPort
{

    #region Methods

    Task PresentRecipeNoteNotFoundAsync(long noteID, CancellationToken cancellationToken);
    Task PresentRecipeNoteRemovedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
