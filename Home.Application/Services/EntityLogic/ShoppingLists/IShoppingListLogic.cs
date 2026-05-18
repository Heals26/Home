using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingLists;

public interface IShoppingListLogic
{

    #region Methods

    ShoppingListItem AddItem(CreateShoppingListItemInputPort inputPort);
    ShoppingListItem GetItem(long shoppingListItemID);
    IQueryable<ShoppingListItem> GetItems(long shoppingListID);
    bool DoesShoppingListExist(long shoppingListID);
    bool DoesShoppingListItemExist(long shoppingListItemID);
    void UpdateItem(UpdateShoppingListItemInputPort inputPort);

    #endregion Methods

}
