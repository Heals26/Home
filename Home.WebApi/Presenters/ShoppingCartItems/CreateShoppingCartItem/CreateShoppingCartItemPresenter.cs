using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCartItems.CreateShoppingCartItem;

namespace Home.WebApi.Presenters.ShoppingCartItems.CreateShoppingCartItem;

public class CreateShoppingCartItemPresenter(IMapper mapper)

    : OutputPortPresenter(mapper), ICreateShoppingCartItemOutputPort
{

    #region  Methods

    Task ICreateShoppingCartItemOutputPort.PresentShoppingCartItemCreatedAsync(long shoppingCartItemID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingCartItemID, new CreateShoppingCartItemApiResponse() { ShoppingCartItemID = shoppingCartItemID }, cancellationToken);

    Task ICreateShoppingCartItemOutputPort.PresentShoppingCartNotFoundAsync(long shoippingCartID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Cart Not Found.", cancellationToken);

    #endregion  Methods

}
