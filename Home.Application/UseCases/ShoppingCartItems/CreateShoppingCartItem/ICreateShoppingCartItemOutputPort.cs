namespace Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;

public interface ICreateShoppingCartItemOutputPort
{

    #region Methods

    Task PresentShoppingCartItemCreatedAsync(long shoppingCartItemID, CancellationToken cancellationToken);
    Task PresentShoppingCartNotFoundAsync(long shoippingCartID, CancellationToken cancellationToken);

    #endregion Methods

}
