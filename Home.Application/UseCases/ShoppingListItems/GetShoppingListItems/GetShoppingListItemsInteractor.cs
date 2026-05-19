using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;

internal class GetShoppingListItemsInteractor : IInteractor<GetShoppingListItemsInputPort, IGetShoppingListItemsOutputPort>
{

    #region Methods

    public Task HandleAsync(
        GetShoppingListItemsInputPort inputPort,
        IGetShoppingListItemsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

        return _ShoppingListLogic.DoesShoppingListExist(inputPort.ShoppingListID)
            ? outputPort.PresentShoppingListItemsAsync(_ShoppingListLogic.GetItems(inputPort.ShoppingListID), cancellationToken)
            : outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
    }

    #endregion Methods

}
