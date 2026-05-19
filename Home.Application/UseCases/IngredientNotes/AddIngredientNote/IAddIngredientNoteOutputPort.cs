namespace Home.Application.UseCases.IngredientNotes.AddIngredientNote;

public interface IAddIngredientNoteOutputPort
{

    #region Methods

    Task PresentIngredientNoteAddedAsync(long noteID, CancellationToken cancellationToken);
    Task PresentIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);

    #endregion Methods

}
