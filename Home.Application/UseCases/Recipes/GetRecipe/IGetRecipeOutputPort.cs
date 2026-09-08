using Home.Application.UseCases.Recipes.Models;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipe;

public interface IGetRecipeOutputPort
{

    #region Methods

    Task PresentRecipeAsync(Recipe recipe, RecipeMealHistory history, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
