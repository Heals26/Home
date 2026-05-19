namespace Home.Application.UseCases.Recipes.CreateRecipe;

public interface ICreateRecipeOutputPort
{

    #region Methods

    Task PresentRecipeCreatedAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
