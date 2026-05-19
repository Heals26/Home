using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.RecipeIngredients.AddRecipeIngredient;

namespace Home.WebApi.Presenters.RecipeIngredients.AddRecipeIngredient;

public class AddRecipeIngredientPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddRecipeIngredientOutputPort
{

    #region Methods

    Task IAddRecipeIngredientOutputPort.PresentRecipeIngredientAddedAsync(long ingredientID, CancellationToken cancellationToken)
        => this.CreatedAsync(ingredientID, new AddRecipeIngredientApiResponse() { IngredientID = ingredientID }, cancellationToken);

    Task IAddRecipeIngredientOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
