using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Recipes.UpdateRecipe;

public class UpdateRecipeWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public PropertyChangeTracker<string> Url { get; set; }

    #endregion Properties

}
