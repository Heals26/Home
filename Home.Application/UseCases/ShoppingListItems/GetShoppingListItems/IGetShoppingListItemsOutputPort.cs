using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;

public interface IGetShoppingListItemsOutputPort
{

    #region Methods

    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);
    Task PresentShoppingListItemsAsync(IQueryable<ShoppingListItem> shoppingListItems, CancellationToken cancellationToken);

    #endregion Methods

}
