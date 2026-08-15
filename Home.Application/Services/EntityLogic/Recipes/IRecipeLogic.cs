using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Recipes;

public interface IRecipeLogic
{

    #region Methods

    /// <summary>
    /// Adds the recipe's ingredients to the list, combining only amounts that share a unit.
    /// A null or empty <paramref name="ingredientIDs"/> means the whole recipe; otherwise only
    /// the ingredients the family ticked are added.
    /// </summary>
    void AddIngredientsToShoppingList(Recipe recipe, ShoppingList shoppingList, IReadOnlyCollection<long>? ingredientIDs);

    #endregion Methods

}
