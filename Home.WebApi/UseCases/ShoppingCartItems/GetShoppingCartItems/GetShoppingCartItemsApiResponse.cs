namespace Home.WebApi.UseCases.ShoppingCartItems.GetShoppingCartItems;

/// <summary>
/// Gets the shopping cart items
/// </summary>
/// <param name="Items"></param>
public record GetShoppingCartItemsApiResponse(
    List<GetShoppingCartItemDto> Items);

/// <summary>
/// The shopping cart items
/// </summary>
/// <param name="Cost"></param>
/// <param name="InBasket"></param>
/// <param name="Name"></param>
/// <param name="Quantity"></param>
/// <param name="Sequence"></param>
public record GetShoppingCartItemDto(
    int Cost,
    bool InBasket,
    string Name,
    int Quantity,
    long Sequence);

