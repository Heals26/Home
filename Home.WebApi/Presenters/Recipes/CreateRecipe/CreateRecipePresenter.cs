using AutoMapper;
using Home.Application.UseCases.Recipes.CreateRecipe;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Recipes.CreateRecipe;

namespace Home.WebApi.Presenters.Recipes.CreateRecipe;

public class CreateRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateRecipeOutputPort
{

    #region Methods

    Task ICreateRecipeOutputPort.PresentRecipeCreatedAsync(long recipeID, CancellationToken cancellationToken)
        => this.CreatedAsync(recipeID, new CreateRecipeApiResponse() { RecipeID = recipeID }, cancellationToken);

    #endregion Methods

}
