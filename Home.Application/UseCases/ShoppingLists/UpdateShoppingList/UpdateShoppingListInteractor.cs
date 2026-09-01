using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.UpdateShoppingList;

internal class UpdateShoppingListInteractor : IInteractor<UpdateShoppingListInputPort, IUpdateShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateShoppingListInputPort inputPort,
        IUpdateShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingList = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_ShoppingList != null)
        {
            if (inputPort.IsArchived.HasBeenSet)
                _ShoppingList.IsArchived = inputPort.IsArchived.Value;

            if (inputPort.Name.HasBeenSet)
                _ShoppingList.Name = inputPort.Name.Value;
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
