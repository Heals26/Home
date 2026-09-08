using AutoMapper;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Application.UseCases.Recipes.Models;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Recipes.GetRecipes;

namespace Home.WebApi.Presenters.Recipes.GetRecipes;

public class GetRecipesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipesOutputPort
{

    #region Methods

    Task IGetRecipesOutputPort.PresentRecipesAsync(IEnumerable<Recipe> recipes, IReadOnlyDictionary<long, RecipeMealHistory> history, CancellationToken cancellationToken)
    {
        var _Response = mapper.Map<GetRecipesApiResponse>(recipes);

        foreach (var _Recipe in _Response.Recipes)
        {
            var _History = history.GetValueOrDefault(_Recipe.RecipeID, RecipeMealHistory.None);

            _Recipe.LastHadDate = _History.LastHadDate;
            _Recipe.NextPlannedDate = _History.NextPlannedDate;
            _Recipe.TimesHad = _History.TimesHad;
        }

        return this.OkAsync(_Response, cancellationToken);
    }

    #endregion Methods

}
