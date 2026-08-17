namespace Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;

public interface IUntickShoppingListItemsOutputPort
{

    #region Methods

    Task PresentShoppingListItemsUntickedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
