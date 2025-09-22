using Home.Application.Services.EntityLogic.ShoppingCarts;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;
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

    ShoppingCartItem IShoppingCartLogic.GetItem(long shoppingCartitemID)
        => persistenceContext.GetEntities<ShoppingCartItem>()
            .Where(sci => sci.ShoppingCartItemID == shoppingCartitemID)
            .Select(sci => new
            {
                ShoppingCartItem = sci
            })
            .Single()
            .ShoppingCartItem;

    IQueryable<ShoppingCartItem> IShoppingCartLogic.GetItems(long shoppingCartID)
        => persistenceContext.GetEntities<ShoppingCartItem>()
            .Where(sc => sc.ShoppingCart.ShoppingCartID == shoppingCartID);

    bool IShoppingCartLogic.DoesShoppingCartExist(long shoppingCartID)
        => persistenceContext.DoesEntityExist<ShoppingCart>(shoppingCartID);

    bool IShoppingCartLogic.DoesShoppingCartItemExist(long shoppingCartItemID)
        => persistenceContext.DoesEntityExist<ShoppingCartItem>(shoppingCartItemID);

    void IShoppingCartLogic.UpdateItem(UpdateShoppingCartItemInputPort inputPort)
    {
        var _ShoppingCartItem = persistenceContext.GetEntities<ShoppingCartItem>()
            .Where(sci => sci.ShoppingCartItemID == inputPort.ShoppingCartItemID)
            .Select(sci => new
            {
                ShoppingCartItem = sci
            })
            .Single()
            .ShoppingCartItem;

        _ = persistenceContext.GetEntities<ShoppingCart>()
            .Where(sc => sc.ShoppingCartID == _ShoppingCartItem.ShoppingCart.ShoppingCartID)
            .ToList();

        if (inputPort.Cost.HasBeenSet)
            _ShoppingCartItem.Cost = inputPort.Cost.Value;

        if (inputPort.InBasket.HasBeenSet)
            _ShoppingCartItem.InBasket = inputPort.InBasket.Value;

        if (inputPort.Name.HasBeenSet)
            _ShoppingCartItem.Name = inputPort.Name.Value;

        if (inputPort.Quantity.HasBeenSet)
            _ShoppingCartItem.Quantity = inputPort.Quantity.Value;

        if (inputPort.Sequence.HasBeenSet)
        {
            _ShoppingCartItem.ShoppingCart.Items
                .Where(i => i.ShoppingCartItemID != _ShoppingCartItem.ShoppingCartItemID && i.Sequence >= inputPort.Sequence.Value)
                .ToList()
                .ForEach(i => i.Sequence++);
        }
    }

    #endregion Methods

}
