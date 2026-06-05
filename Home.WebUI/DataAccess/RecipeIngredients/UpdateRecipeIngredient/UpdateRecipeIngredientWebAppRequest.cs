using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.RecipeIngredients.UpdateRecipeIngredient;

public class UpdateRecipeIngredientWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// The quantity of the ingredient.
    /// </summary>
    public PropertyChangeTracker<decimal?> Quantity { get; set; }

    /// <summary>
    /// The volume of the ingredient.
    /// </summary>
    public PropertyChangeTracker<decimal?> Volume { get; set; }

    /// <summary>
    /// The weight of the ingredient.
    /// </summary>
    public PropertyChangeTracker<decimal?> Weight { get; set; }

    #endregion Properties

}
