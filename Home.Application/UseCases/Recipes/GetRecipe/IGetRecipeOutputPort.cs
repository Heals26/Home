using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipe;

public interface IGetRecipeOutputPort
{

    #region Methods

    Task PresentRecipeAsync(Recipe recipe, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
