namespace Home.Application.Infrastructure.Recipes;

/// <summary>
/// A line of a recipe's ingredient list, split into the parts the app stores.
/// </summary>
/// <param name="Amount">How much, or null when the line never said.</param>
/// <param name="Name">What to buy, with the amount and unit taken off the front.</param>
/// <param name="Unit">The measurement the amount is in, or null for a plain count.</param>
public record ParsedIngredient(decimal? Amount, string Name, long? Unit);
