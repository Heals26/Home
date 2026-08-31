using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeIngredients.SetRecipeIngredientSequence;

public class SetRecipeIngredientSequencePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetRecipeIngredientSequenceOutputPort
{

    #region Methods

    Task ISetRecipeIngredientSequenceOutputPort.PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Ingredient {ingredientID} Not Found", cancellationToken);

    Task ISetRecipeIngredientSequenceOutputPort.PresentRecipeIngredientSequenceSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
