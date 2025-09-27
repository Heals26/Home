namespace Home.Application.UseCases.ShoppingCartItems.DeleteShoppingCartItem;

public interface IDeleteShoppingCartItemOutputPort
{

    #region Methods

    Task PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken);
    Task PresentShoppingCartItemNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
