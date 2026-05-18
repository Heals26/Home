namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;

/// <summary>
/// Gets the item in the shopping list
/// </summary>
public record GetShoppingListItemApiResponse(
    decimal? Cost,
    bool InBasket,
    string Name,
    decimal? Quantity,
    long Sequence,
    long ShoppingListItemID,
    decimal? Volume,
    decimal? Weight);
