namespace Home.Application.UseCases.ShoppingLists.CreateShoppingList;

public interface ICreateShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListCreatedAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
