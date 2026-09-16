using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.StartShoppingTrip;

/// <summary>
/// Switches shopping mode on. Whoever is already shopping with the list is joined rather than given
/// a second trip, because two people who split up at the door are one shop.
/// </summary>
internal class StartShoppingTripInteractor : IInteractor<StartShoppingTripInputPort, IStartShoppingTripOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        StartShoppingTripInputPort inputPort,
        IStartShoppingTripOutputPort outputPort,
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
            .SingleOrDefault();

        if (_ShoppingList == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        var _Trip = _TripLogic.FindOpen(_Household, _ShoppingList.ShoppingListID);

        if (_Trip == null)
        {
            _Trip = await _TripLogic.StartAsync(_Household, _ShoppingList, cancellationToken);
            _AuditLogic.UpdateAudit(_ShoppingList, $"started shopping with '{_ShoppingList.Name}'");
        }
        else
        {
            _TripLogic.RecordActivity(_Trip);
            _AuditLogic.UpdateAudit(_ShoppingList, $"joined in shopping with '{_ShoppingList.Name}'");
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingTripAsync(_Trip, cancellationToken);
    }

    #endregion Methods

}
