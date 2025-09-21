using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingCarts;

public interface IShoppingCartLogic
{

    #region  Methods

    ShoppingCartItem AddItem(CreateShoppingCartItemInputPort inputPort);
    Task GetItem(long shoppingCartItemID);

    #endregion  Methods

}
