using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.ShoppingLists;

public class ShoppingListLogic(IPersistenceContext persistenceContext) : IShoppingListLogic
{

    #region Methods

    ShoppingListItem IShoppingListLogic.AddItem(CreateShoppingListItemInputPort inputPort)
    {
        var _ShoppingList = persistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .Single()
            .ShoppingList;

        return new ShoppingListItem()
        {
            Amount = inputPort.Amount,
            Cost = inputPort.Cost,
            InBasket = inputPort.InBasket,
            Name = inputPort.Name,
            Note = inputPort.Note,
            // Counting rather than taking the highest reuses a sequence after any deletion.
            Sequence = (_ShoppingList.Items?.Max(i => (long?)i.Sequence) ?? 0) + 1,
            Unit = inputPort.Unit
        };
    }

    bool IShoppingListLogic.DoesShoppingListExist(long shoppingListID)
        => persistenceContext.DoesEntityExist<ShoppingList>(shoppingListID);

    bool IShoppingListLogic.DoesShoppingListItemExist(long shoppingListItemID)
        => persistenceContext.DoesEntityExist<ShoppingListItem>(shoppingListItemID);

    ShoppingListItem? IShoppingListLogic.GetItem(long shoppingListItemID)
        => persistenceContext.Find<ShoppingListItem>(shoppingListItemID);

    IQueryable<ShoppingListItem> IShoppingListLogic.GetItems(long shoppingListID)
        => persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingList.ShoppingListID == shoppingListID)
            .OrderBy(sli => sli.Sequence)
            .ThenBy(sli => sli.ShoppingListItemID);

    void IShoppingListLogic.UpdateItem(UpdateShoppingListItemInputPort inputPort)
    {
        var _ShoppingListItem = persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingListItemID == inputPort.ShoppingListItemID)
            .Select(sli => new
            {
                ShoppingListItem = sli,
                sli.ShoppingList,
                sli.ShoppingList.Items
            })
            .Single()
            .ShoppingListItem;

        if (inputPort.Amount.HasBeenSet)
            _ShoppingListItem.Amount = inputPort.Amount.Value;

        if (inputPort.Cost.HasBeenSet)
            _ShoppingListItem.Cost = inputPort.Cost.Value;

        if (inputPort.InBasket.HasBeenSet)
            _ShoppingListItem.InBasket = inputPort.InBasket.Value;

        if (inputPort.Name.HasBeenSet)
            _ShoppingListItem.Name = inputPort.Name.Value;

        // Blank comes back as nothing at all, so an emptied box leaves a clean line rather than a
        // note that is there but says nothing.
        if (inputPort.Note.HasBeenSet)
            _ShoppingListItem.Note = string.IsNullOrWhiteSpace(inputPort.Note.Value) ? null : inputPort.Note.Value.Trim();

        if (inputPort.Unit.HasBeenSet)
            _ShoppingListItem.Unit = inputPort.Unit.Value;

        // Set, not insert. This used to shuffle every item at or after the target down by one and
        // never assign the moved item its own sequence, so a reorder pushed the rest of the list
        // apart and left the item exactly where it started. The caller reorders by swapping a pair,
        // the same way the recipe ingredient list does, and a swap needs each half written where
        // the other one was.
        if (inputPort.Sequence.HasBeenSet)
            _ShoppingListItem.Sequence = inputPort.Sequence.Value;
    }

    #endregion Methods

}
