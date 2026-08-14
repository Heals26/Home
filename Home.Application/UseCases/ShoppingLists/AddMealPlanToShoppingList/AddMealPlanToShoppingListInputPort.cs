using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;

public record AddMealPlanToShoppingListInputPort(
    DateTime FromDate,
    long ShoppingListID,
    DateTime ToDate)
    : IInputPort<IAddMealPlanToShoppingListOutputPort>;
