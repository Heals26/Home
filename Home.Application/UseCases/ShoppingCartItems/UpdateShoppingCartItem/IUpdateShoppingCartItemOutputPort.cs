namespace Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;

public interface IUpdateShoppingCartItemOutputPort
{

    #region Methods

    Task PresentShoppingCartItemNoContentAsync(CancellationToken cancellationToken);
    Task PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken);

    #endregion Methods

}
