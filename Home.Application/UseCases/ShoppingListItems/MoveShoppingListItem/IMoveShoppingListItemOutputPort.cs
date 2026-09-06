namespace Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;

public interface IMoveShoppingListItemOutputPort
{

    #region Methods

    Task PresentShoppingListItemMovedAsync(CancellationToken cancellationToken);
    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
