using CleanArchitecture.Mediator;
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
                .ForEach(sli => sli.InBasket = false);

            _AuditLogic.UpdateAudit(_ShoppingList);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentShoppingListItemsUntickedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
