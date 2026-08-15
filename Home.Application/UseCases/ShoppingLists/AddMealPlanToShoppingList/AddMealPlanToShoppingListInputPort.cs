using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;

/// <summary>
/// A null <see cref="MealSlotID"/> takes every meal in the window; naming one takes only that
/// meal, so shopping for the week's dinners does not also buy the week's breakfasts.
/// </summary>
public record AddMealPlanToShoppingListInputPort(
    DateTime FromDate,
    long? MealSlotID,
    long ShoppingListID,
    DateTime ToDate)
    : IInputPort<IAddMealPlanToShoppingListOutputPort>;
