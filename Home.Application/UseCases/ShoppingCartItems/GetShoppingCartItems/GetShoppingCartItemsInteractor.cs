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

        if (!_ShoppingCartLogic.DoesShoppingCartExist(inputPort.ShoppingCartID))
            return outputPort.PresentShoppingCartNotFoundAsync(inputPort.ShoppingCartID, cancellationToken);

        return outputPort.PresentShoppingCartItemsAsync(_ShoppingCartLogic.GetItems(inputPort.ShoppingCartID), cancellationToken);
    }

    #endregion Methods

}
