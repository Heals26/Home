using Home.WebApi.UseCases.Ingredients.Models;

namespace Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;

public class GetRecipeIngredientApiResponse
{

    #region Properties

    public long IngredientID { get; set; }
    public string Name { get; set; }
    public List<IngredientNoteDto> Notes { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
