namespace Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;

public interface ICreateShoppingListItemOutputPort
{

    #region Methods

    Task PresentShoppingListItemCreatedAsync(long shoppingListItemID, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
