using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingLists.UpdateShoppingList;

public record UpdateShoppingListInputPort(PropertyChangeTracker<string> Name, long ShoppingListID) : IInputPort<IUpdateShoppingListOutputPort>;
