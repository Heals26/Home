using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeIngredients.UpdateRecipeIngredient;

public class UpdateRecipeIngredientPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateRecipeIngredientOutputPort
{

    #region Methods

    Task IUpdateRecipeIngredientOutputPort.PresentRecipeIngredientNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateRecipeIngredientOutputPort.PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Ingredient {ingredientID} Not Found", cancellationToken);

    #endregion Methods

}
