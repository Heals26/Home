using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeIngredients.RemoveRecipeIngredient;

public class RemoveRecipeIngredientPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRemoveRecipeIngredientOutputPort
{

    #region Methods

    Task IRemoveRecipeIngredientOutputPort.PresentRecipeIngredientRemovedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IRemoveRecipeIngredientOutputPort.PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Ingredient {ingredientID} Not Found", cancellationToken);

    #endregion Methods

}
