namespace Home.Application.Services.RecipeImports;

/// <summary>
/// A recipe as read from someone else's website — names and free text only. Quantities stay
/// inside the ingredient line ("2 cups flour") because splitting them reliably is a losing game.
/// </summary>
public record ImportedRecipe(
    string Name,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<ImportedRecipeStep> Steps);
