namespace Home.WebApi.UseCases.ShoppingLists.AddRecipeToShoppingList;

/// <summary>
/// The body is optional — sending none, or an empty list, adds the whole recipe.
/// </summary>
public record AddRecipeToShoppingListApiRequest(List<long> IngredientIDs);
