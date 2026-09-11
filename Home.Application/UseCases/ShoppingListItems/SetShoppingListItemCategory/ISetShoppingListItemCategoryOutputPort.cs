namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;

public interface ISetShoppingListItemCategoryOutputPort
{

    #region Methods

    Task PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken);
    Task PresentShoppingListItemCategorySetAsync(CancellationToken cancellationToken);
    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);

    #endregion Methods

}
