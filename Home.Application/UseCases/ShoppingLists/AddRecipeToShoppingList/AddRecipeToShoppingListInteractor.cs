using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Recipes;
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
        var _RecipeLogic = serviceFactory.GetService<IRecipeLogic>();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID)
            .Select(r => new
            {
                Recipe = r,
                Ingredients = r.Ingredients.Select(ri => new
                {
                    RecipeIngredient = ri,
                    ri.Ingredient
                })
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
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_ShoppingList == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        _RecipeLogic.AddIngredientsToShoppingList(_Recipe, _ShoppingList);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeAddedToShoppingListAsync(cancellationToken);
    }

    #endregion Methods

}
