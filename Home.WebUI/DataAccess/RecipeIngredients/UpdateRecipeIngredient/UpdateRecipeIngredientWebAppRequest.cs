using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.RecipeIngredients.UpdateRecipeIngredient;

public class UpdateRecipeIngredientWebAppRequest
{

    #region Properties

    /// <summary>
    /// How much of the ingredient, in <see cref="Unit"/>.
    /// </summary>
    public PropertyChangeTracker<decimal?> Amount { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// The measurement the amount is in.
    /// </summary>
    public PropertyChangeTracker<long?> Unit { get; set; }

    #endregion Properties

}
