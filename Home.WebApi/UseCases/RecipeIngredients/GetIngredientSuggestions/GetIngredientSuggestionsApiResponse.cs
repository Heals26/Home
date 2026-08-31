namespace Home.WebApi.UseCases.RecipeIngredients.GetIngredientSuggestions;

public class GetIngredientSuggestionsApiResponse
{

    #region Properties

    /// <summary>
    /// Ingredients the household has cooked with before, most used first
    /// </summary>
    public ICollection<GetIngredientSuggestionDto> Suggestions { get; set; }

    #endregion Properties

}
