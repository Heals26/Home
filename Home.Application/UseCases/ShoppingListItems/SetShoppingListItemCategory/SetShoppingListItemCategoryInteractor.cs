using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;

internal class SetShoppingListItemCategoryInteractor : IInteractor<SetShoppingListItemCategoryInputPort, ISetShoppingListItemCategoryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetShoppingListItemCategoryInputPort inputPort,
        ISetShoppingListItemCategoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();
        var _MemoryLogic = serviceFactory.GetService<IShoppingItemMemoryLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingListItem = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(i => i.ShoppingListItemID == inputPort.ShoppingListItemID
                && i.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .Select(i => new
            {
                ShoppingListItem = i,
                i.ShoppingList
            })
            .SingleOrDefault()
            ?.ShoppingListItem;

        var _ShoppingCategory = inputPort.ShoppingCategoryID is { } _ShoppingCategoryID
            ? _PersistenceContext.GetEntities<ShoppingCategory>()
                .Where(c => c.ShoppingCategoryID == _ShoppingCategoryID
                    && c.Household.HouseholdID == _Household.HouseholdID)
                .SingleOrDefault()
            : null;

        if (_ShoppingListItem == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
        }
        else if (inputPort.ShoppingCategoryID is { } _MissingID && _ShoppingCategory == null)
        {
            await outputPort.PresentShoppingCategoryNotFoundAsync(_MissingID, cancellationToken);
        }
        else
        {
            // Filed against the name rather than the line, so the same item lands in the same aisle on
            // every list from now on.
            var _Memory = await _MemoryLogic.GetOrCreateAsync(_Household, _ShoppingListItem.Name, cancellationToken);

            _Memory.ShoppingCategory = _ShoppingCategory;

            _AuditLogic.UpdateAudit(_ShoppingListItem.ShoppingList, _ShoppingCategory == null
                ? $"took '{_ShoppingListItem.Name}' out of its aisle"
                : $"put '{_ShoppingListItem.Name}' in {_ShoppingCategory.Name}");

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingListItemCategorySetAsync(cancellationToken);
        }
    }

    #endregion Methods

}
