using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItem;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCartItems.GetShoppingCartItem;

namespace Home.WebApi.Presenters.ShoppingCartItems.GetShoppingCart;

public class GetShoppingCartItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingCartItemOutputPort
{

    #region Methods

    Task IGetShoppingCartItemOutputPort.PresentShoppingCartItemAsync(ShoppingCartItem shoppingCartItem, CancellationToken cancellationToken) =>
        this.OkAsync(new GetShoppingCartItemApiResponse(
            shoppingCartItem.Cost,
            shoppingCartItem.InBasket,
            shoppingCartItem.Name,
            shoppingCartItem.Quantity,
            shoppingCartItem.Sequence),
            cancellationToken);

    Task IGetShoppingCartItemOutputPort.PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Cart Not Found", cancellationToken);

    #endregion Methods

}
