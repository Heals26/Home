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

    ShoppingListItem? IShoppingListLogic.GetItem(Household household, long shoppingListItemID)
        => persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingListItemID == shoppingListItemID
                && sli.ShoppingList.Household.HouseholdID == household.HouseholdID)
            .Select(sli => new
            {
                ShoppingListItem = sli,
                sli.ShoppingList,
                sli.ShoppingList.Items
            })
            .SingleOrDefault()
            ?.ShoppingListItem;

    IQueryable<ShoppingListItem> IShoppingListLogic.GetItems(long shoppingListID)
        => persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingList.ShoppingListID == shoppingListID)
            .OrderBy(sli => sli.Sequence)
            .ThenBy(sli => sli.ShoppingListItemID);

    /// <summary>
    /// This used to shuffle the items at or after the target down by one and never assign the moved
    /// item its own sequence, so a reorder pushed the list apart and left the item exactly where it
    /// started. Doing it in one place server-side, rather than as a pair of swaps from the caller, is
    /// what lets a list be reordered by dropping an item anywhere in it and not only by nudging it
    /// past its neighbour.
    /// </summary>
    void IShoppingListLogic.MoveItem(ShoppingListItem shoppingListItem, long sequence)
    {
        if (sequence == shoppingListItem.Sequence)
            return;

        var _From = shoppingListItem.Sequence;

        foreach (var _Other in shoppingListItem.ShoppingList.Items
            .Where(i => i.ShoppingListItemID != shoppingListItem.ShoppingListItemID))
        {
            if (sequence > _From && _Other.Sequence > _From && _Other.Sequence <= sequence)
                _Other.Sequence--;
            else if (sequence < _From && _Other.Sequence >= sequence && _Other.Sequence < _From)
                _Other.Sequence++;
        }

        shoppingListItem.Sequence = sequence;
    }

    void IShoppingListLogic.UpdateItem(ShoppingListItem shoppingListItem, UpdateShoppingListItemInputPort inputPort)
    {
        if (inputPort.Amount.HasBeenSet)
            shoppingListItem.Amount = inputPort.Amount.Value;

        if (inputPort.Cost.HasBeenSet)
            shoppingListItem.Cost = inputPort.Cost.Value;

        if (inputPort.Name.HasBeenSet)
            shoppingListItem.Name = inputPort.Name.Value;

        // Blank comes back as nothing at all, so an emptied box leaves a clean line rather than a
        // note that is there but says nothing.
        if (inputPort.Note.HasBeenSet)
            shoppingListItem.Note = string.IsNullOrWhiteSpace(inputPort.Note.Value) ? null : inputPort.Note.Value.Trim();

        if (inputPort.Unit.HasBeenSet)
            shoppingListItem.Unit = inputPort.Unit.Value;
    }

    #endregion Methods

}
