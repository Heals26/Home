using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;

namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

internal class UpdateShoppingListItemInteractor : IInteractor<UpdateShoppingListItemInputPort, IUpdateShoppingListItemOutputPort>
{

    #region Methods

    public Task HandleAsync(
        UpdateShoppingListItemInputPort inputPort,
        IUpdateShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

        if (!_ShoppingListLogic.DoesShoppingListItemExist(inputPort.ShoppingListItemID))
            return outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);

        _ShoppingListLogic.UpdateItem(inputPort);

        return outputPort.PresentShoppingListItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
