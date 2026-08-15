namespace Home.Application.Services.RecipeImports;

/// <summary>
/// A recipe as read from someone else's website — names and free text only. Quantities stay
/// inside the ingredient line ("2 cups flour") because splitting them reliably is a losing game.
/// The times and yield are whatever the page claimed, or null when it claimed nothing readable.
/// </summary>
public record ImportedRecipe(
    int? CookMinutes,
    string? ImageUrl,
    IReadOnlyList<string> Ingredients,
    string Name,
    int? PrepMinutes,
    int? Servings,
    IReadOnlyList<ImportedRecipeStep> Steps);
