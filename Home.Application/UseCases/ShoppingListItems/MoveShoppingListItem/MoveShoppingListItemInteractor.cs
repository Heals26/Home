using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;

internal class MoveShoppingListItemInteractor
    : IInteractor<MoveShoppingListItemInputPort, IMoveShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        MoveShoppingListItemInputPort inputPort,
        IMoveShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // The list it is going to, with its items, because the moved item needs a position at the
        // end of it and that means knowing what is already there.
        var _Target = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_Target == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        // The list it is leaving comes with it, because assigning the new one is what detaches it
        // and an unloaded navigation would be a null the save cannot resolve.
        var _Item = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingListItemID == inputPort.ShoppingListItemID
                && sli.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .Select(sli => new
            {
                ShoppingListItem = sli,
                sli.ShoppingList
            })
            .SingleOrDefault()
            ?.ShoppingListItem;

        if (_Item == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
            return;
        }

        if (_Item.ShoppingList.ShoppingListID != _Target.ShoppingListID)
        {
            // Onto the end, the same place a newly added item goes. Dropping onto a list is saying
            // which list, not where in it.
            _Item.Sequence = (_Target.Items?.Max(i => (long?)i.Sequence) ?? 0) + 1;
            _Item.ShoppingList = _Target;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentShoppingListItemMovedAsync(cancellationToken);
    }

    #endregion Methods

}
