using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;

internal class DeleteShoppingCategoryInteractor : IInteractor<DeleteShoppingCategoryInputPort, IDeleteShoppingCategoryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingCategoryInputPort inputPort,
        IDeleteShoppingCategoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCategory>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingCategory = _PersistenceContext.GetEntities<ShoppingCategory>()
            .Where(c => c.ShoppingCategoryID == inputPort.ShoppingCategoryID
                && c.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_ShoppingCategory == null)
        {
            await outputPort.PresentShoppingCategoryNotFoundAsync(inputPort.ShoppingCategoryID, cancellationToken);
        }
        else
        {
            // Filed items cannot cascade from here, and clearing a navigation EF never loaded changes
            // nothing, so they are loaded with their aisle and let go of it.
            var _Filed = _PersistenceContext.GetEntities<ShoppingItemMemory>()
                .Where(m => m.Household.HouseholdID == _Household.HouseholdID
                    && m.ShoppingCategory != null
                    && m.ShoppingCategory.ShoppingCategoryID == _ShoppingCategory.ShoppingCategoryID)
                .Select(m => new
                {
                    Memory = m,
                    m.ShoppingCategory
                })
                .ToList()
                .Select(m => m.Memory)
                .ToList();

            foreach (var _Memory in _Filed)
                _Memory.ShoppingCategory = null;

            _AuditLogic.DeleteAudit(_ShoppingCategory);
            _PersistenceContext.Remove(_ShoppingCategory);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingCategoryDeletedAsync(cancellationToken);
        }
    }

    #endregion Methods

}
