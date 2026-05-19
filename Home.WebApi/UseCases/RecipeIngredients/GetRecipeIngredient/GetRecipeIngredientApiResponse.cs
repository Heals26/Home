using Home.WebApi.UseCases.Ingredients.Models;

namespace Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;

public class GetRecipeIngredientApiResponse
{

    #region Properties

    /// <summary>
    /// The ID of the ingredient.
    /// </summary>
    public long IngredientID { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Notes that have been added to this ingredient.
    /// </summary>
    public List<IngredientNoteDto> Notes { get; set; }

    /// <summary>
    /// The quantity of the ingredient (unit-agnostic count).
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// The volume measurement of the ingredient.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// The weight measurement of the ingredient.
    /// </summary>
    public decimal? Weight { get; set; }

    #endregion Properties

}
