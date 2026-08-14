using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;

internal class DeleteShoppingListItemInteractor : IInteractor<DeleteShoppingListItemInputPort, IDeleteShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingListItemInputPort inputPort,
        IDeleteShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingListItem = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(i => i.ShoppingListItemID == inputPort.ShoppingListItemID
                && i.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_ShoppingListItem == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ShoppingListItem);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
