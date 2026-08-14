using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
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
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Item = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(i => i.ShoppingListItemID == inputPort.ShoppingListItemID
                && i.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Item == null)
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
        else
            await outputPort.PresentShoppingListItemAsync(_Item, cancellationToken);
    }

    #endregion Methods

}
