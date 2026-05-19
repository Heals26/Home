using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;

internal class GetShoppingListItemInteractor : IInteractor<GetShoppingListItemInputPort, IGetShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingListItemInputPort inputPort,
        IGetShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

        var _Item = _ShoppingListLogic.GetItem(inputPort.ShoppingListItemID);

        if (_Item == null)
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
        else
            await outputPort.PresentShoppingListItemAsync(_Item, cancellationToken);
    }

    #endregion Methods

}
