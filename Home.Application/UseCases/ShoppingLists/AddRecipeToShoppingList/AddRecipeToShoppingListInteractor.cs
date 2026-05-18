using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;

internal class AddRecipeToShoppingListInteractor : IInteractor<AddRecipeToShoppingListInputPort, IAddRecipeToShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddRecipeToShoppingListInputPort inputPort,
        IAddRecipeToShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID)
            .Select(r => new
            {
                Recipe = r,
                Ingredients = r.Ingredients.Select(ri => new { RecipeIngredient = ri, ri.Ingredient })
            })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _ShoppingList = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID)
            .Select(sl => new { ShoppingList = sl, sl.Items })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_ShoppingList == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        foreach (var _RecipeIngredient in _Recipe.Ingredients)
        {
            var _Ingredient = _RecipeIngredient.Ingredient;
            var _ExistingItem = _ShoppingList.Items
                .FirstOrDefault(i => string.Equals(i.Name, _Ingredient.Name, StringComparison.OrdinalIgnoreCase));

            if (_ExistingItem != null)
            {
                _ExistingItem.Quantity = CombineValues(_ExistingItem.Quantity, _Ingredient.Quantity);
                _ExistingItem.Volume = CombineValues(_ExistingItem.Volume, _Ingredient.Volume);
                _ExistingItem.Weight = CombineValues(_ExistingItem.Weight, _Ingredient.Weight);
            }
            else
            {
                _ShoppingList.Items.Add(new ShoppingListItem()
                {
                    Name = _Ingredient.Name,
                    Quantity = _Ingredient.Quantity,
                    Volume = _Ingredient.Volume,
                    Weight = _Ingredient.Weight,
                    InBasket = false,
                    Sequence = _ShoppingList.Items.Count + 1
                });
            }
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeAddedToShoppingListAsync(cancellationToken);
    }

    private static decimal? CombineValues(decimal? existing, decimal? incoming)
    {
        if (existing == null && incoming == null)
            return null;

        return (existing ?? 0) + (incoming ?? 0);
    }

    #endregion Methods

}
