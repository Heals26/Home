using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;

internal class UpdateShoppingCategoryInteractor : IInteractor<UpdateShoppingCategoryInputPort, IUpdateShoppingCategoryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateShoppingCategoryInputPort inputPort,
        IUpdateShoppingCategoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCategory>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _HouseholdCategories = _PersistenceContext.GetEntities<ShoppingCategory>()
            .Where(c => c.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        var _ShoppingCategory = _HouseholdCategories.SingleOrDefault(c => c.ShoppingCategoryID == inputPort.ShoppingCategoryID);
        var _Name = inputPort.Name.HasBeenSet ? inputPort.Name.Value.Trim() : null;

        if (_ShoppingCategory == null)
        {
            await outputPort.PresentShoppingCategoryNotFoundAsync(inputPort.ShoppingCategoryID, cancellationToken);
        }
        else if (_Name != null && _HouseholdCategories.Any(c => c.ShoppingCategoryID != _ShoppingCategory.ShoppingCategoryID
            && string.Equals(c.Name, _Name, StringComparison.OrdinalIgnoreCase)))
        {
            await outputPort.PresentShoppingCategoryNameConflictAsync(_Name, cancellationToken);
        }
        else
        {
            // A rename is history. A reorder is housekeeping, and a row for every aisle each time the
            // shop is rearranged would drown the feed.
            if (_Name != null && _Name != _ShoppingCategory.Name)
            {
                _ShoppingCategory.Name = _Name;
                _AuditLogic.UpdateAudit(_ShoppingCategory);
            }

            if (inputPort.Sequence.HasBeenSet)
                _ShoppingCategory.Sequence = inputPort.Sequence.Value;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingCategoryNoContentAsync(cancellationToken);
        }
    }

    #endregion Methods

}
