namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

/// <summary>
/// Something the household has bought before, offered back when the next list is being written.
/// </summary>
/// <param name="Amount">The amount it was last added with, so the usual size comes back with it.</param>
/// <param name="Cost">What it last cost, so a list totals up without anyone typing prices twice.</param>
/// <param name="Name">The item as it was last written.</param>
/// <param name="TimesAdded">How often it has been added, which is the order these are offered in.</param>
/// <param name="Unit">The measurement the amount was last in.</param>
public record ShoppingListItemSuggestion(
    decimal? Amount,
    decimal? Cost,
    string Name,
    long TimesAdded,
    long? Unit);
