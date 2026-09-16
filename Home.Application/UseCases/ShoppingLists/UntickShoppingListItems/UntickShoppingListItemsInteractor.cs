using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;

/// <summary>
/// Puts a standing list back to the start of the week without anyone retyping it.
/// </summary>
internal class UntickShoppingListItemsInteractor : IInteractor<UntickShoppingListItemsInputPort, IUntickShoppingListItemsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UntickShoppingListItemsInputPort inputPort,
        IUntickShoppingListItemsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();
        var _TripLogic = serviceFactory.GetService<IShoppingTripLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingList = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_ShoppingList != null)
        {
            // The shop ends before anything comes out of the trolley, so what was bought on it stands.
            _ = _TripLogic.End(_Household, _ShoppingList.ShoppingListID);

            foreach (var _Item in _ShoppingList.Items.Where(sli => sli.InBasket))
            {
                _Item.InBasket = false;
                _Item.ShoppingTripID = null;
            }

            _AuditLogic.UpdateAudit(_ShoppingList, $"unticked everything on '{_ShoppingList.Name}'");

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentShoppingListItemsUntickedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
