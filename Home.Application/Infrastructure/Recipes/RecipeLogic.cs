using Home.Application.Services.EntityLogic.Recipes;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Recipes;

public class RecipeLogic : IRecipeLogic
{

    #region Methods

    void IRecipeLogic.AddIngredientsToShoppingList(Recipe recipe, ShoppingList shoppingList, IReadOnlyCollection<long>? ingredientIDs)
    {
        var _Chosen = ingredientIDs == null || ingredientIDs.Count == 0
            ? recipe.Ingredients
            : recipe.Ingredients.Where(ri => ingredientIDs.Contains(ri.IngredientID)).ToList();

        foreach (var _RecipeIngredient in _Chosen)
        {
            var _Ingredient = _RecipeIngredient.Ingredient;

            // Only amounts measured the same way may be added together — two cups and two
            // hundred grams of the same thing are two lines, not 202 of nothing.
            var _ExistingItem = shoppingList.Items
                .FirstOrDefault(i => string.Equals(i.Name, _Ingredient.Name, StringComparison.OrdinalIgnoreCase)
                    && i.Unit == _Ingredient.Unit);

            if (_ExistingItem != null)
                _ExistingItem.Amount = CombineValues(_ExistingItem.Amount, _Ingredient.Amount);
            else
                shoppingList.Items.Add(new ShoppingListItem()
                {
                    Amount = _Ingredient.Amount,
                    InBasket = false,
                    Name = _Ingredient.Name,
                    Sequence = shoppingList.Items.Count + 1,
                    Unit = _Ingredient.Unit
                });
        }
    }

    private static decimal? CombineValues(decimal? existing, decimal? incoming)
    {
        if (existing == null && incoming == null)
            return null;

        return (existing ?? 0) + (incoming ?? 0);
    }

    #endregion Methods

}
