using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCartItems.CreateShoppingCartItem;

public class CreateShoppingCartItemPresenter(IMapper mapper)
    : OutputPortPresenter(IMapper mapper), ICreateShoppingCartItemOutputPort
{

    #region  Methods

    Task ICreateShoppingCartItemOutputPort.PresentShoppingCartItemCreatedAsync(long shoppingCartItemID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingCartItemID, new CreateShoppingCartItemApiResponse() { ShoppingCartItemID = shoppingCartItemID }, cancellationToken);

    #endregion  Methods

}
