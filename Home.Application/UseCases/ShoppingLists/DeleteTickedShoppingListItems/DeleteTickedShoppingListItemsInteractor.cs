using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;

/// <summary>
/// Closes off a shop in one call. Doing it item by item from the phone would be a round trip per
/// line, which is the difference between the list emptying and the list draining.
/// </summary>
internal class DeleteTickedShoppingListItemsInteractor : IInteractor<DeleteTickedShoppingListItemsInputPort, IDeleteTickedShoppingListItemsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteTickedShoppingListItemsInputPort inputPort,
        IDeleteTickedShoppingListItemsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();

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
            _ShoppingList.Items
                .Where(sli => sli.InBasket)
                .ToList()
                .ForEach(_PersistenceContext.Remove);

            _AuditLogic.UpdateAudit(_ShoppingList);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentTickedShoppingListItemsDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
