using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingLists;

public interface IShoppingListLogic
{

    #region Methods

    ShoppingListItem AddItem(CreateShoppingListItemInputPort inputPort);
    bool DoesShoppingListExist(long shoppingListID);
    bool DoesShoppingListItemExist(long shoppingListItemID);
    ShoppingListItem? GetItem(long shoppingListItemID);

    /// <summary>
    /// The line, its list and the rest of the list around it, and only when this household owns it.
    /// Everything that writes to a single line reads it through here, so the household check and the
    /// loading are in one place rather than one per endpoint.
    /// </summary>
    ShoppingListItem? GetItem(Household household, long shoppingListItemID);

    IQueryable<ShoppingListItem> GetItems(long shoppingListID);

    /// <summary>
    /// Takes the line out of the order and puts it back at <paramref name="sequence"/>, closing the
    /// gap it left and opening one where it lands.
    /// </summary>
    void MoveItem(ShoppingListItem shoppingListItem, long sequence);

    void UpdateItem(ShoppingListItem shoppingListItem, UpdateShoppingListItemInputPort inputPort);

    #endregion Methods

}
