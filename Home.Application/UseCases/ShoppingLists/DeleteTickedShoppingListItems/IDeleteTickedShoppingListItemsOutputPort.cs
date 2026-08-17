namespace Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;

public interface IDeleteTickedShoppingListItemsOutputPort
{

    #region Methods

    Task PresentTickedShoppingListItemsDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
