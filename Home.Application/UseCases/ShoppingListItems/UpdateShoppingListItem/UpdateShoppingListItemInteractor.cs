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
        var _MemoryLogic = serviceFactory.GetService<IShoppingItemMemoryLogic>();
        var _TripLogic = serviceFactory.GetService<IShoppingTripLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Before = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(i => i.ShoppingListItemID == inputPort.ShoppingListItemID
                && i.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .Select(i => new
            {
                i.InBasket,
                i.ShoppingList.ShoppingListID
            })
            .SingleOrDefault();

        if (_Before == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
            return;
        }

        var _ShoppingListItem = _ShoppingListLogic.UpdateItem(inputPort);
        var _Trip = _TripLogic.FindOpen(_Household, _Before.ShoppingListID);

        if (_Trip != null)
            _TripLogic.RecordActivity(_Trip);

        // A note or a new place in the list changes nothing the household has paid for.
        if (inputPort.InBasket.HasBeenSet || inputPort.Cost.HasBeenSet || inputPort.Amount.HasBeenSet || inputPort.Unit.HasBeenSet || inputPort.Name.HasBeenSet)
            await _MemoryLogic.RecordTickAsync(_Household, _ShoppingListItem, _Before.InBasket, _Trip, cancellationToken);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
