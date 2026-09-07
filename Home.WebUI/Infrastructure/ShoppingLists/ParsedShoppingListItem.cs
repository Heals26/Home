namespace Home.WebUI.Infrastructure.ShoppingLists;

/// <summary>
/// What someone typed, split into the parts an item is made of.
/// </summary>
/// <param name="Amount">How much to buy, or null when no amount was written.</param>
/// <param name="Name">The thing to buy, always exactly what was typed minus the amount.</param>
/// <param name="Unit">The measurement the amount is in, or null for a plain count.</param>
public record ParsedShoppingListItem(decimal? Amount, string Name, long? Unit);
