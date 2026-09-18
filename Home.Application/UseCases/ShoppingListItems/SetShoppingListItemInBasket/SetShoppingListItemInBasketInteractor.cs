using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;

internal class SetShoppingListItemInBasketInteractor : IInteractor<SetShoppingListItemInBasketInputPort, ISetShoppingListItemInBasketOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetShoppingListItemInBasketInputPort inputPort,
        ISetShoppingListItemInBasketOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();
        var _MemoryLogic = serviceFactory.GetService<IShoppingItemMemoryLogic>();
        var _TripLogic = serviceFactory.GetService<IShoppingTripLogic>();

        var _Household = _AuthorisationService.GetHousehold();
        var _ShoppingListItem = _ShoppingListLogic.GetItem(_Household, inputPort.ShoppingListItemID);

        if (_ShoppingListItem == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
        }
        else
        {
            var _WasInBasket = _ShoppingListItem.InBasket;

            _ShoppingListItem.InBasket = inputPort.InBasket;

            var _Trip = _TripLogic.RecordActivityOn(_Household, _ShoppingListItem.ShoppingList.ShoppingListID);

            await _MemoryLogic.RecordTickAsync(_Household, _ShoppingListItem, _WasInBasket, _Trip, cancellationToken);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingListItemInBasketSetAsync(cancellationToken);
        }
    }

    #endregion Methods

}
