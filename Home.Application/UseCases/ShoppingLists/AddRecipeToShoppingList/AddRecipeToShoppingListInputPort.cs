using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;

/// <summary>
/// A null or empty <see cref="IngredientIDs"/> means the whole recipe; otherwise only the
/// ingredients the family ticked are added.
/// </summary>
public record AddRecipeToShoppingListInputPort(
    IReadOnlyList<long>? IngredientIDs,
    long RecipeID,
    long ShoppingListID)
    : IInputPort<IAddRecipeToShoppingListOutputPort>;
