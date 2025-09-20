using AutoMapper;
using Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCarts.CreateShoppingCart;

namespace Home.WebApi.Presenters.ShoppingCarts.CreateShoppingCart;

public class CreateShoppingCartPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateShoppingCartOutputPort
{

    #region Methods

    Task ICreateShoppingCartOutputPort.PresentShoppingCartCreatedAsync(long shoppingCartID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingCartID, new CreateShoppingCartApiResponse() { ShoppingCartID = shoppingCartID }, cancellationToken);

    #endregion Methods

}
