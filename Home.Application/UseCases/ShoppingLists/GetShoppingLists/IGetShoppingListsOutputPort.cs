using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingLists;

public interface IGetShoppingListsOutputPort
{

    #region Methods

    Task PresentShoppingListsAsync(IEnumerable<ShoppingList> shoppingLists, CancellationToken cancellationToken);

    #endregion Methods

}
