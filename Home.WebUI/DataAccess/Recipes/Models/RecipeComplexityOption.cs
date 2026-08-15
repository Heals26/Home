namespace Home.WebUI.DataAccess.Recipes.Models;

/// <summary>
/// One entry in the "how involved is this?" picker.
/// </summary>
/// <param name="Name">How the level reads on screen.</param>
/// <param name="Value">The stored value the API exchanges.</param>
public record RecipeComplexityOption(string Name, long Value);
