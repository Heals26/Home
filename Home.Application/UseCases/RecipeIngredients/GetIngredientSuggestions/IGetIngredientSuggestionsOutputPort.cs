namespace Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;

public interface IGetIngredientSuggestionsOutputPort
{

    #region Methods

    Task PresentIngredientSuggestionsAsync(IEnumerable<IngredientSuggestion> suggestions, CancellationToken cancellationToken);

    #endregion Methods

}
