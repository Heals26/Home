namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;

public interface ISetShoppingListItemInBasketOutputPort
{

    #region Methods

    Task PresentShoppingListItemInBasketSetAsync(CancellationToken cancellationToken);
    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);

    #endregion Methods

}
