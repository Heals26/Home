using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

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
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingListExists = _PersistenceContext.GetEntities<ShoppingList>()
            .Any(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID);

        return _ShoppingListExists
            ? outputPort.PresentShoppingListItemsAsync(_ShoppingListLogic.GetItems(inputPort.ShoppingListID), cancellationToken)
            : outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
    }

    #endregion Methods

}
