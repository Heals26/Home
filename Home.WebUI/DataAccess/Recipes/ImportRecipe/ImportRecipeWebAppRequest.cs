namespace Home.WebUI.DataAccess.Recipes.ImportRecipe;

public class ImportRecipeWebAppRequest
{

    #region Properties

    /// <summary>
    /// The full http or https address of the recipe page to import.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    #endregion Properties

}
