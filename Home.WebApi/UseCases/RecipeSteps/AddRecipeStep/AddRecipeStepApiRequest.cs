namespace Home.WebApi.UseCases.RecipeSteps.AddRecipeStep;

public class AddRecipeStepApiRequest
{

    #region Properties

    /// <summary>
    /// The full text content of the step.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The ID of the recipe this step belongs to.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// The order in which this step should appear in the recipe.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// A short title summarising the step.
    /// </summary>
    public string Title { get; set; }

    #endregion Properties

}
