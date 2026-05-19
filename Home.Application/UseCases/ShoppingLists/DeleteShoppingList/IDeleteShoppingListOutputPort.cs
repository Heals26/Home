namespace Home.Application.UseCases.ShoppingLists.DeleteShoppingList;

public interface IDeleteShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
