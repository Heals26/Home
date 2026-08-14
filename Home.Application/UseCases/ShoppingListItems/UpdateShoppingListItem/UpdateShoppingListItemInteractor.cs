using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

internal class UpdateShoppingListItemInteractor : IInteractor<UpdateShoppingListItemInputPort, IUpdateShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateShoppingListItemInputPort inputPort,
        IUpdateShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingListItemExists = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Any(i => i.ShoppingListItemID == inputPort.ShoppingListItemID
                && i.ShoppingList.Household.HouseholdID == _Household.HouseholdID);

        if (!_ShoppingListItemExists)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
            return;
        }

        _ShoppingListLogic.UpdateItem(inputPort);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
