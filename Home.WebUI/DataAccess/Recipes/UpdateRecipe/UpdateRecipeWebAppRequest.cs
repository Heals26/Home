using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Recipes.UpdateRecipe;

public class UpdateRecipeWebAppRequest
{

    #region Properties

    /// <summary>
    /// How involved the recipe is.
    /// </summary>
    public PropertyChangeTracker<long?> Complexity { get; set; }

    /// <summary>
    /// Minutes on the stove.
    /// </summary>
    public PropertyChangeTracker<int?> CookMinutes { get; set; }

    /// <summary>
    /// A picture of the finished dish.
    /// </summary>
    public PropertyChangeTracker<string> ImageUrl { get; set; }

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Minutes of chopping and measuring before cooking starts.
    /// </summary>
    public PropertyChangeTracker<int?> PrepMinutes { get; set; }

    /// <summary>
    /// How many the recipe feeds.
    /// </summary>
    public PropertyChangeTracker<int?> Servings { get; set; }

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public PropertyChangeTracker<string> Url { get; set; }

    #endregion Properties

}
