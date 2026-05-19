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
            Cost = inputPort.Cost,
            InBasket = inputPort.InBasket,
            Name = inputPort.Name,
            Quantity = inputPort.Quantity,
            Sequence = (_ShoppingList.Items?.Count + 1) ?? 1,
            Volume = inputPort.Volume,
            Weight = inputPort.Weight
        };
    }

    bool IShoppingListLogic.DoesShoppingListExist(long shoppingListID)
        => persistenceContext.DoesEntityExist<ShoppingList>(shoppingListID);

    bool IShoppingListLogic.DoesShoppingListItemExist(long shoppingListItemID)
        => persistenceContext.DoesEntityExist<ShoppingListItem>(shoppingListItemID);

    ShoppingListItem IShoppingListLogic.GetItem(long shoppingListItemID)
        => persistenceContext.Find<ShoppingListItem>(shoppingListItemID);

    IQueryable<ShoppingListItem> IShoppingListLogic.GetItems(long shoppingListID)
        => persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingList.ShoppingListID == shoppingListID);

    void IShoppingListLogic.UpdateItem(UpdateShoppingListItemInputPort inputPort)
    {
        var _ShoppingListItem = persistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingListItemID == inputPort.ShoppingListItemID)
            .Select(sli => new
            {
                ShoppingListItem = sli
            })
            .Single()
            .ShoppingListItem;

        _ = persistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == _ShoppingListItem.ShoppingList.ShoppingListID)
            .ToList();

        if (inputPort.Cost.HasBeenSet)
            _ShoppingListItem.Cost = inputPort.Cost.Value;

        if (inputPort.InBasket.HasBeenSet)
            _ShoppingListItem.InBasket = inputPort.InBasket.Value;

        if (inputPort.Name.HasBeenSet)
            _ShoppingListItem.Name = inputPort.Name.Value;

        if (inputPort.Quantity.HasBeenSet)
            _ShoppingListItem.Quantity = inputPort.Quantity.Value;

        if (inputPort.Volume.HasBeenSet)
            _ShoppingListItem.Volume = inputPort.Volume.Value;

        if (inputPort.Weight.HasBeenSet)
            _ShoppingListItem.Weight = inputPort.Weight.Value;

        if (inputPort.Sequence.HasBeenSet)
        {
            _ShoppingListItem.ShoppingList.Items
                .Where(i => i.ShoppingListItemID != _ShoppingListItem.ShoppingListItemID && i.Sequence >= inputPort.Sequence.Value)
                .ToList()
                .ForEach(i => i.Sequence++);
        }
    }

    #endregion Methods

}
