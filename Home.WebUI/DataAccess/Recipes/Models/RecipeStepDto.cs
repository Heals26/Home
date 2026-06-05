namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeStepDto
{

    #region Properties

    /// <summary>
    /// The full text content of the step.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the recipe step.
    /// </summary>
    public long RecipeStepID { get; set; }

    /// <summary>
    /// The order in which this step appears in the recipe.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// A short title summarising the step.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
