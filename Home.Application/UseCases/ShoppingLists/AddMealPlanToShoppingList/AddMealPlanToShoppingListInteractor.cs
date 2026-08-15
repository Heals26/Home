using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Recipes;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;

/// <summary>
/// Adds every ingredient from the recipes planned in a date window to one shopping list —
/// the "put the week's cooking on the list" tap. A recipe planned twice in the window is
/// added once; doubling quantities is a decision the family makes at the shop, not here.
/// </summary>
internal class AddMealPlanToShoppingListInteractor
    : IInteractor<AddMealPlanToShoppingListInputPort, IAddMealPlanToShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddMealPlanToShoppingListInputPort inputPort,
        IAddMealPlanToShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _RecipeLogic = serviceFactory.GetService<IRecipeLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingList = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID
                && sl.Household.HouseholdID == _Household.HouseholdID)
            .Select(sl => new { ShoppingList = sl, sl.Items })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_ShoppingList == null)
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        var _FromDate = inputPort.FromDate.Date;
        var _ToDate = inputPort.ToDate.Date;

        var _Recipes = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.Recipe.Household.HouseholdID == _Household.HouseholdID
                && e.Date >= _FromDate
                && e.Date <= _ToDate
                && (inputPort.MealSlotID == null
                    || (e.MealSlot != null && e.MealSlot.MealSlotID == inputPort.MealSlotID)))
            .Select(e => new
            {
                e.Recipe,
                Ingredients = e.Recipe.Ingredients.Select(ri => new { RecipeIngredient = ri, ri.Ingredient })
            })
            .ToList()
            .Select(e => e.Recipe)
            .DistinctBy(r => r.RecipeID)
            .ToList();

        foreach (var _Recipe in _Recipes)
            _RecipeLogic.AddIngredientsToShoppingList(_Recipe, _ShoppingList, null);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealPlanAddedToShoppingListAsync(_Recipes.Count, cancellationToken);
    }

    #endregion Methods

}
