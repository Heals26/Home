namespace Home.WebUI.DataAccess.RecipeIngredients.GetIngredientSuggestions;

public class GetIngredientSuggestionsWebAppResponse
{

    #region Properties

    /// <summary>
    /// Ingredients the household has cooked with before, most used first.
    /// </summary>
    public ICollection<GetIngredientSuggestionDto> Suggestions { get; set; } = [];

    #endregion Properties

}
