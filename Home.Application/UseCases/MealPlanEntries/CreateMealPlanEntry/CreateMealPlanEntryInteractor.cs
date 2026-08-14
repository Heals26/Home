using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

internal class CreateMealPlanEntryInteractor
    : IInteractor<CreateMealPlanEntryInputPort, ICreateMealPlanEntryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateMealPlanEntryInputPort inputPort,
        ICreateMealPlanEntryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // Scoped to the household so one family cannot plan another family's recipe.
        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID && r.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Entry = new MealPlanEntry()
        {
            Date = inputPort.Date.Date,
            Recipe = _Recipe
        };

        _PersistenceContext.Add(_Entry);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealPlanEntryCreatedAsync(_Entry.MealPlanEntryID, cancellationToken);
    }

    #endregion Methods

}
