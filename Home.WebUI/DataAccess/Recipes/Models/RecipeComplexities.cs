namespace Home.WebUI.DataAccess.Recipes.Models;

/// <summary>
/// A mirror of the API's complexity enumeration, kept here because the web app does not
/// reference the domain. The values must stay in step with Home.Domain's RecipeComplexitySE.
/// </summary>
public static class RecipeComplexities
{

    #region Fields

    public static readonly IReadOnlyList<RecipeComplexityOption> All =
    [
        new("Easy", 1),
        new("Moderate", 2),
        new("Involved", 3),
    ];

    #endregion Fields

    #region Methods

    public static string GetName(long? complexity)
        => All.FirstOrDefault(c => c.Value == complexity)?.Name ?? string.Empty;

    #endregion Methods

}
