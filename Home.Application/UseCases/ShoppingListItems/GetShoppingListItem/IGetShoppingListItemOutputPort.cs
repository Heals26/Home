using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;

public interface IGetShoppingListItemOutputPort
{

    #region Methods

    Task PresentShoppingListItemAsync(ShoppingListItem shoppingListItem, CancellationToken cancellationToken);
    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);

    #endregion Methods

}
