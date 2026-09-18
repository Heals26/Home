using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;

internal class SetShoppingListItemSequenceInteractor : IInteractor<SetShoppingListItemSequenceInputPort, ISetShoppingListItemSequenceOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetShoppingListItemSequenceInputPort inputPort,
        ISetShoppingListItemSequenceOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();
        var _TripLogic = serviceFactory.GetService<IShoppingTripLogic>();

        var _Household = _AuthorisationService.GetHousehold();
        var _ShoppingListItem = _ShoppingListLogic.GetItem(_Household, inputPort.ShoppingListItemID);

        if (_ShoppingListItem == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
        }
        else
        {
            _ShoppingListLogic.MoveItem(_ShoppingListItem, inputPort.Sequence);

            _ = _TripLogic.RecordActivityOn(_Household, _ShoppingListItem.ShoppingList.ShoppingListID);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingListItemSequenceSetAsync(cancellationToken);
        }
    }

    #endregion Methods

}
