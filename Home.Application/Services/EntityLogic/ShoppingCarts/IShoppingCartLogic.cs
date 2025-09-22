using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingCarts;

public interface IShoppingCartLogic
{

    #region  Methods

    ShoppingCartItem AddItem(CreateShoppingCartItemInputPort inputPort);
    ShoppingCartItem GetItem(long shoppingCartItemID);
    IQueryable<ShoppingCartItem> GetItems(long shoppingCartItemID);
    bool DoesShoppingCartExist(long shoppingCartID);
    bool DoesShoppingCartItemExist(long shoppingCartItemID);
    void UpdateItem(UpdateShoppingCartItemInputPort inputPort);

    #endregion  Methods

}
