using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;

/// <summary>
/// Ticks the line into the trolley, or takes it back out. Its own use case rather than a field on
/// the patch, because a tick during a shop is a purchase and an untick takes that purchase back,
/// which is nothing like editing what the line says.
/// </summary>
public record SetShoppingListItemInBasketInputPort(bool InBasket, long ShoppingListItemID) : IInputPort<ISetShoppingListItemInBasketOutputPort>;
