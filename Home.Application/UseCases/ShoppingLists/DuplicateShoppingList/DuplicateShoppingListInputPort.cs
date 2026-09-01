using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.DuplicateShoppingList;

public record DuplicateShoppingListInputPort(string Name, long ShoppingListID) : IInputPort<IDuplicateShoppingListOutputPort>;
