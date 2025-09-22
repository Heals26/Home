using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingCarts;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItem;

internal class GetShoppingCartItemInteractor : IInteractor<GetShoppingCartItemInputPort, IGetShoppingCartItemOutputPort>
{

    #region Methods

    public Task HandleAsync(
        GetShoppingCartItemInputPort inputPort,
        IGetShoppingCartItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingCartLogic = serviceFactory.GetService<IShoppingCartLogic>();

        return Task.FromResult(_ShoppingCartLogic.GetItem(inputPort.shoppingCartItem));
    }

    #endregion Methods

}
