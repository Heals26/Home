using Home.Application.Services.EntityLogic.ShoppingCarts;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.ShoppingCarts;

public class ShoppingCartLogic(IPersistenceContext persistenceContext) : IShoppingCartLogic
{

    #region Methods

    ShoppingCartItem IShoppingCartLogic.AddItem(CreateShoppingCartItemInputPort inputPort)
    {
        var _ShoppingCart = persistenceContext.GetEntities<ShoppingCart>()
            .Where(sc => sc.ShoppingCartID == inputPort.ShoppingCartID)
            .Select(sc => new
            {
                ShoppingCart = sc,
                sc.Items
            })
            .Single()
            .ShoppingCart;

        return new ShoppingCartItem()
        {
            ShoppingCartItemID = inputPort.ShoppingCartID,
            Quantity = inputPort.Quantity,
            Name = inputPort.Name,
            Cost = inputPort.Cost,
            Sequence = (_ShoppingCart.Items?.Count + 1) ?? 1
        };
    }

    Task IShoppingCartLogic.GetItem(long shoppingCartitemID)
    {

    }

    #endregion Methods

}
