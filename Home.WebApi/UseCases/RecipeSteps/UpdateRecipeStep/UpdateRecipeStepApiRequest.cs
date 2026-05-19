using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.RecipeSteps.UpdateRecipeStep;

public class UpdateRecipeStepApiRequest
{

    #region Properties

    /// <summary>
    /// The updated full text content of the step.
    /// </summary>
    public PropertyChangeTracker<string> Content { get; set; }

    /// <summary>
    /// The updated sequence order of the step.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    /// <summary>
    /// The updated short title of the step.
    /// </summary>
    public PropertyChangeTracker<string> Title { get; set; }

    #endregion Properties

}
