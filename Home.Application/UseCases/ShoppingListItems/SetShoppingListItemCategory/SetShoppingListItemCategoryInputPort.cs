using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;

/// <summary>
/// Files the item under an aisle, or takes it out of one when <paramref name="ShoppingCategoryID"/>
/// is null.
/// </summary>
public record SetShoppingListItemCategoryInputPort(long? ShoppingCategoryID, long ShoppingListItemID) : IInputPort<ISetShoppingListItemCategoryOutputPort>;
