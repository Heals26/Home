using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.DuplicateShoppingList;

internal class DuplicateShoppingListInteractor : IInteractor<DuplicateShoppingListInputPort, IDuplicateShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DuplicateShoppingListInputPort inputPort,
        IDuplicateShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Source = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_Source == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
        }
        else
        {
            // Copied in one call rather than one call per line, for the same reason clearing a
            // list is one call: thirty round trips over a supermarket connection is the difference
            // between a list appearing and a list filling in.
            //
            // Nothing arrives ticked and nothing arrives priced. "This week's like last week's"
            // means the same things to buy, not last week's trolley or last week's receipt.
            var _Duplicate = new ShoppingList()
            {
                Household = _Household,
                IsArchived = false,
                Items = [.. _Source.Items.OrderBy(i => i.Sequence).Select(i => new ShoppingListItem()
                {
                    Amount = i.Amount,
                    InBasket = false,
                    Name = i.Name,
                    Sequence = i.Sequence,
                    Unit = i.Unit
                })],
                Name = inputPort.Name
            };

            _PersistenceContext.Add(_Duplicate);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentShoppingListDuplicatedAsync(_Duplicate.ShoppingListID, cancellationToken);
        }
    }

    #endregion Methods

}
