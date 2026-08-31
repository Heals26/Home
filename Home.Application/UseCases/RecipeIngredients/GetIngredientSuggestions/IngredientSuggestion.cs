namespace Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;

/// <summary>
/// Something the household has cooked with before, offered back while a recipe is being written.
/// </summary>
/// <param name="Amount">The amount it was last written with, so the usual quantity comes back with it.</param>
/// <param name="Name">The ingredient as it was last written.</param>
/// <param name="TimesUsed">How many recipes it appears in, which is the order these are offered in.</param>
/// <param name="Unit">The measurement the amount was last in.</param>
public record IngredientSuggestion(
    decimal? Amount,
    string Name,
    long TimesUsed,
    long? Unit);
