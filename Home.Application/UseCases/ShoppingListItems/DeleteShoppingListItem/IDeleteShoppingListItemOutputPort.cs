namespace Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;

public interface IDeleteShoppingListItemOutputPort
{

    #region Methods

    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);
    Task PresentShoppingListItemNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
