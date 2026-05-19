using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.CreateShoppingList;

internal class CreateShoppingListInteractor : IInteractor<CreateShoppingListInputPort, ICreateShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateShoppingListInputPort inputPort,
        ICreateShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _ShoppingList = new ShoppingList()
        {
            Household = _AuthorisationService.GetHousehold(),
            Items = [],
            Name = inputPort.Name
        };

        _AuditLogic.AddAudit(_ShoppingList);

        _PersistenceContext.Add(_ShoppingList);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListCreatedAsync(_ShoppingList.ShoppingListID, cancellationToken);
    }

    #endregion Methods

}
