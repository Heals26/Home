namespace Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;

public interface IAddRecipeToShoppingListOutputPort
{

    #region Methods

    Task PresentRecipeAddedToShoppingListAsync(CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
