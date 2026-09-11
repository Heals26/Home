using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;

internal class CreateShoppingCategoryInteractor : IInteractor<CreateShoppingCategoryInputPort, ICreateShoppingCategoryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateShoppingCategoryInputPort inputPort,
        ICreateShoppingCategoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCategory>>();

        var _Household = _AuthorisationService.GetHousehold();
        var _Name = inputPort.Name.Trim();

        var _Existing = _PersistenceContext.GetEntities<ShoppingCategory>()
            .Where(c => c.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        // The database holds a unique index on household and name; catching it here gives the family
        // a readable answer instead of a failed save.
        if (_Existing.Any(c => string.Equals(c.Name, _Name, StringComparison.OrdinalIgnoreCase)))
        {
            await outputPort.PresentShoppingCategoryNameConflictAsync(_Name, cancellationToken);
        }
        else
        {
            var _ShoppingCategory = new ShoppingCategory()
            {
                Household = _Household,
                Name = _Name,
                Sequence = _Existing.Count == 0 ? 0 : _Existing.Max(c => c.Sequence) + 1
            };

            _PersistenceContext.Add(_ShoppingCategory);
            _AuditLogic.AddAudit(_ShoppingCategory);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingCategoryCreatedAsync(_ShoppingCategory.ShoppingCategoryID, cancellationToken);
        }
    }

    #endregion Methods

}
