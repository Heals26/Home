namespace Home.Application.UseCases.Recipes.UpdateRecipe;

public interface IUpdateRecipeOutputPort
{

    #region Methods

    Task PresentRecipeNoContentAsync(CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
