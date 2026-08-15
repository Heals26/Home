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
            {
                _ExistingItem.Amount = CombineValues(_ExistingItem.Amount, _Ingredient.Amount);

                // Rows written before amounts carried a unit still hold the old columns, so
                // those keep adding up until every row has moved across.
                if (_ExistingItem.Amount == null)
                {
                    _ExistingItem.Quantity = CombineValues(_ExistingItem.Quantity, _Ingredient.Quantity);
                    _ExistingItem.Volume = CombineValues(_ExistingItem.Volume, _Ingredient.Volume);
                    _ExistingItem.Weight = CombineValues(_ExistingItem.Weight, _Ingredient.Weight);
                }
            }
            else
            {
                var _IsLegacy = _Ingredient.Amount == null;

                shoppingList.Items.Add(new ShoppingListItem()
                {
                    Amount = _Ingredient.Amount,
                    InBasket = false,
                    Name = _Ingredient.Name,
                    Quantity = _IsLegacy ? _Ingredient.Quantity : null,
                    Sequence = shoppingList.Items.Count + 1,
                    Unit = _Ingredient.Unit,
                    Volume = _IsLegacy ? _Ingredient.Volume : null,
                    Weight = _IsLegacy ? _Ingredient.Weight : null
                });
            }
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
