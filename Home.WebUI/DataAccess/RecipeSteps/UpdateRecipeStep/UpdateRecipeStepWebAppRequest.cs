using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.RecipeSteps.UpdateRecipeStep;

public class UpdateRecipeStepWebAppRequest
{

    #region Properties

    /// <summary>
    /// The full text content of the step.
    /// </summary>
    public PropertyChangeTracker<string> Content { get; set; }

    /// <summary>
    /// The order in which this step appears in the recipe.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    /// <summary>
    /// A short title summarising the step.
    /// </summary>
    public PropertyChangeTracker<string> Title { get; set; }

    #endregion Properties

}
