using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.DeleteShoppingList;

internal class DeleteShoppingListInteractor : IInteractor<DeleteShoppingListInputPort, IDeleteShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingListInputPort input,
        IDeleteShoppingListOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();

        var _Household = _AuthorisationService.GetHousehold();

        _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == input.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .ToList()
            .ForEach(sl =>
            {
                _PersistenceContext.Remove(sl);
                _AuditLogic.DeleteAudit(sl);
            });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentShoppingListDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
