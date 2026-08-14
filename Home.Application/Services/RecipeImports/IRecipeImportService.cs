namespace Home.Application.Services.RecipeImports;

public interface IRecipeImportService
{

    #region Methods

    /// <summary>
    /// Reads a recipe from a web page. Null when the page is unreachable or carries nothing
    /// recognisable as a recipe — an unreadable page is a return value here, not an exception.
    /// </summary>
    Task<ImportedRecipe?> FetchRecipeAsync(string url, CancellationToken cancellationToken);

    #endregion Methods

}
