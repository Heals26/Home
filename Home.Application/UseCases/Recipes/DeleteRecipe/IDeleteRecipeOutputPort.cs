namespace Home.Application.UseCases.Recipes.DeleteRecipe;

public interface IDeleteRecipeOutputPort
{

    #region Methods

    Task PresentRecipeDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
