namespace Home.Application.UseCases.ShoppingLists.DuplicateShoppingList;

public interface IDuplicateShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListDuplicatedAsync(long shoppingListID, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
