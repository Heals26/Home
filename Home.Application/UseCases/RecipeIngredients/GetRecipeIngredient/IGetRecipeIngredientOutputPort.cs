using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;

public interface IGetRecipeIngredientOutputPort
{

    #region Methods

    Task PresentRecipeIngredientAsync(Ingredient ingredient, CancellationToken cancellationToken);
    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);

    #endregion Methods

}
