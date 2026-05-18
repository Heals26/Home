using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;

public record AddRecipeToShoppingListInputPort(long RecipeID, long ShoppingListID) : IInputPort<IAddRecipeToShoppingListOutputPort>;
