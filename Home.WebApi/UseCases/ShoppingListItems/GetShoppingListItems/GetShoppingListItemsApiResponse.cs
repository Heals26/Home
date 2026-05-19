namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;

/// <summary>
/// Gets the shopping list items
/// </summary>
public record GetShoppingListItemsApiResponse(List<GetShoppingListItemDto> Items);

/// <summary>
/// A shopping list item
/// </summary>
public record GetShoppingListItemDto(
    decimal? Cost,
    bool InBasket,
    string Name,
    decimal? Quantity,
    long Sequence,
    long ShoppingListItemID,
    decimal? Volume,
    decimal? Weight);
