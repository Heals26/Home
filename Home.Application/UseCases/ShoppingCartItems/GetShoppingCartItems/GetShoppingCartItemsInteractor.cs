using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingCarts;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItems;

public class GetShoppingCartItemsInteractor : IInteractor<GetShoppingCartItemsInputPort, IGetShoppingCartItemsOutputPort>
{

    #region Methods

    public Task HandleAsync(
        GetShoppingCartItemsInputPort inputPort,
        IGetShoppingCartItemsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingCartLogic = serviceFactory.GetService<IShoppingCartLogic>();

        return _ShoppingCartLogic.DoesShoppingCartExist(inputPort.ShoppingCartID)
            ? outputPort.PresentShoppingCartItemsAsync(_ShoppingCartLogic.GetItems(inputPort.ShoppingCartID), cancellationToken)
            : outputPort.PresentShoppingCartNotFoundAsync(inputPort.ShoppingCartID, cancellationToken);
    }

    #endregion Methods

}
