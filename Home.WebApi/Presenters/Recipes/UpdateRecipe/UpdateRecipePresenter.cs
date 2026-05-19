using AutoMapper;
using Home.Application.UseCases.Recipes.UpdateRecipe;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Recipes.UpdateRecipe;

public class UpdateRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateRecipeOutputPort
{

    #region Methods

    Task IUpdateRecipeOutputPort.PresentRecipeNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateRecipeOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
