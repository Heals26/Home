using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipe;

internal class GetRecipeInteractor : IInteractor<GetRecipeInputPort, IGetRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipeInputPort inputPort,
        IGetRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID && r.Household.HouseholdID == _Household.HouseholdID)
            .Select(r => new
            {
                Recipe = r,
                Ingredients = r.Ingredients.Select(ri => new
                {
                    RecipeIngredient = ri,
                    ri.Ingredient
                }),
                MealSlots = r.MealSlots.Select(rms => new
                {
                    RecipeMealSlot = rms,
                    rms.MealSlot
                }),
                Notes = r.Notes.Select(rn => new
                {
                    RecipeNote = rn,
                    rn.Note
                }),
                r.Steps
            })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
        else
            await outputPort.PresentRecipeAsync(_Recipe, cancellationToken);
    }

    #endregion Methods

}
