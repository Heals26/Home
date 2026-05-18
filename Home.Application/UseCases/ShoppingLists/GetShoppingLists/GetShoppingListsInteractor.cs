using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingLists;

internal class GetShoppingListsInteractor : IInteractor<GetShoppingListsInputPort, IGetShoppingListsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingListsInputPort input,
        IGetShoppingListsOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingLists = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.Household.HouseholdID == _Household.HouseholdID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .ToList()
            .Select(sl => sl.ShoppingList);

        await output.PresentShoppingListsAsync(_ShoppingLists, cancellationToken).ConfigureAwait(false);
    }

    #endregion Methods

}
