namespace Home.WebUI.DataAccess.ShoppingCartItems.GetShoppingCartItem;

/// <summary>
/// Gets the item in the shopping cart
/// </summary>
/// <param name="Cost"></param>
/// <param name="InBasket"></param>
/// <param name="Name"></param>
/// <param name="Quantity"></param>
/// <param name="Sequence"></param>
public record GetShoppingCartItemWebAppResponse(
        int Cost,
        bool InBasket,
        string Name,
        int Quantity,
        long Sequence);

