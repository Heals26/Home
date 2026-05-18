using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingList;

public interface IGetShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListAsync(ShoppingList shoppingList, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
