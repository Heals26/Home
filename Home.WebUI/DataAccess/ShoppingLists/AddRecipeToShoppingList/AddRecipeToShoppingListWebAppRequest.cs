namespace Home.WebUI.DataAccess.ShoppingLists.AddRecipeToShoppingList;

public class AddRecipeToShoppingListWebAppRequest
{

    #region Properties

    /// <summary>
    /// The ingredients to add. An empty list adds the whole recipe.
    /// </summary>
    public List<long> IngredientIDs { get; set; } = [];

    #endregion Properties

}
