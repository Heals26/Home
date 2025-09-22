using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingCarts;

namespace Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;

public class UpdateShoppingCartItemInteractor : IInteractor<UpdateShoppingCartItemInputPort, IUpdateShoppingCartItemOutputPort>
{

    #region Methods

    public Task HandleAsync(
        UpdateShoppingCartItemInputPort inputPort,
        IUpdateShoppingCartItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingCartLogic = serviceFactory.GetService<IShoppingCartLogic>();

        if (!_ShoppingCartLogic.DoesShoppingCartItemExist(inputPort.ShoppingCartItemID))
            return outputPort.PresentShoppingCartItemNotFoundAsync(inputPort.ShoppingCartItemID, cancellationToken);

        _ShoppingCartLogic.UpdateItem(inputPort);

        return outputPort.PresentShoppingCartItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
