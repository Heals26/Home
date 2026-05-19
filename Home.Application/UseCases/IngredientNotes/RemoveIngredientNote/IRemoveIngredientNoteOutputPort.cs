namespace Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;

public interface IRemoveIngredientNoteOutputPort
{

    #region Methods

    Task PresentIngredientNoteNotFoundAsync(long noteID, CancellationToken cancellationToken);
    Task PresentIngredientNoteRemovedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
