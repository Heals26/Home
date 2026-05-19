using Home.Application.Services.EntityLogic.Recipes;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Recipes;

public class RecipeLogic : IRecipeLogic
{

    #region Methods

    void IRecipeLogic.AddIngredientsToShoppingList(Recipe recipe, ShoppingList shoppingList)
    {
        foreach (var _RecipeIngredient in recipe.Ingredients)
        {
            var _Ingredient = _RecipeIngredient.Ingredient;
            var _ExistingItem = shoppingList.Items
                .FirstOrDefault(i => string.Equals(i.Name, _Ingredient.Name, StringComparison.OrdinalIgnoreCase));

            if (_ExistingItem != null)
            {
                _ExistingItem.Quantity = CombineValues(_ExistingItem.Quantity, _Ingredient.Quantity);
                _ExistingItem.Volume = CombineValues(_ExistingItem.Volume, _Ingredient.Volume);
                _ExistingItem.Weight = CombineValues(_ExistingItem.Weight, _Ingredient.Weight);
            }
            else
            {
                shoppingList.Items.Add(new ShoppingListItem()
                {
                    Name = _Ingredient.Name,
                    Quantity = _Ingredient.Quantity,
                    Volume = _Ingredient.Volume,
                    Weight = _Ingredient.Weight,
                    InBasket = false,
                    Sequence = shoppingList.Items.Count + 1
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
