using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.RecipeIngredients.GetIngredientSuggestions;

namespace Home.WebApi.Presenters.RecipeIngredients.GetIngredientSuggestions;

public class GetIngredientSuggestionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetIngredientSuggestionsOutputPort
{

    #region Methods

    Task IGetIngredientSuggestionsOutputPort.PresentIngredientSuggestionsAsync(IEnumerable<IngredientSuggestion> suggestions, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetIngredientSuggestionsApiResponse>(suggestions), cancellationToken);

    #endregion Methods

}
