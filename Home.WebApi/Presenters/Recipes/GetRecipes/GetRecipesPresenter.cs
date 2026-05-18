using AutoMapper;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Recipes.GetRecipes;

namespace Home.WebApi.Presenters.Recipes.GetRecipes;

public class GetRecipesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipesOutputPort
{

    #region Methods

    Task IGetRecipesOutputPort.PresentRecipesAsync(IEnumerable<Recipe> recipes, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetRecipesApiResponse>(recipes), cancellationToken);

    #endregion Methods

}
