using Home.WebApi.UseCases.Recipes.Models;

namespace Home.WebApi.UseCases.Recipes.GetRecipe;

public class GetRecipeApiResponse
{

    #region Properties

    public List<RecipeIngredientDto> Ingredients { get; set; }
    public string Name { get; set; }
    public List<RecipeNoteDto> Notes { get; set; }
    public long RecipeID { get; set; }
    public List<RecipeStepDto> Steps { get; set; }
    public string Url { get; set; }

    #endregion Properties

}
