using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.EndShoppingTrip;

/// <summary>
/// Done, from any phone in the shop. The trip ends for everyone shopping with the list, and what was
/// bought on it stands.
/// </summary>
internal class EndShoppingTripInteractor : IInteractor<EndShoppingTripInputPort, IEndShoppingTripOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        EndShoppingTripInputPort inputPort,
        IEndShoppingTripOutputPort outputPort,
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

        // Two phones can both press Done, and the second finds nothing left to end.
        if (_ShoppingList != null && _TripLogic.End(_Household, _ShoppingList.ShoppingListID))
        {
            _AuditLogic.UpdateAudit(_ShoppingList, $"finished shopping with '{_ShoppingList.Name}'");

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentShoppingTripEndedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
