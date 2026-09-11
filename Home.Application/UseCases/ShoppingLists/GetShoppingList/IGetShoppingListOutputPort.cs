using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingList;

public interface IGetShoppingListOutputPort
{

    #region Methods

    Task PresentShoppingListAsync(ShoppingList shoppingList, IReadOnlyDictionary<long, ShoppingItemInsight> insights, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
