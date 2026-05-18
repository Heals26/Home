namespace Home.Application.UseCases.ShoppingLists.UpdateShoppingList;

public interface IUpdateShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
