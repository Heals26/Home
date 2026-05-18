namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

public interface IUpdateShoppingListItemOutputPort
{

    #region Methods

    Task PresentShoppingListItemNoContentAsync(CancellationToken cancellationToken);
    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);

    #endregion Methods

}
