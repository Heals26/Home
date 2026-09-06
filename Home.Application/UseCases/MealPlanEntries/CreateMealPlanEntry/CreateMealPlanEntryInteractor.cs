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

        var _Title = inputPort.Title?.Trim();

        Recipe? _Recipe = null;

        if (inputPort.RecipeID != null)
        {
            // Scoped to the household so one family cannot plan another family's recipe.
            _Recipe = _PersistenceContext.GetEntities<Recipe>()
                .Where(r => r.RecipeID == inputPort.RecipeID && r.Household.HouseholdID == _Household.HouseholdID)
                .SingleOrDefault();

            if (_Recipe == null)
            {
                await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID.Value, cancellationToken);
                return;
            }

            // The recipe's own name is the answer, so a title alongside it would be a second one
            // that goes stale the moment the recipe is renamed.
            _Title = null;
        }
        else if (string.IsNullOrWhiteSpace(_Title))
        {
            await outputPort.PresentNothingToPlanAsync(cancellationToken);
            return;
        }

        MealSlot? _MealSlot = null;

        if (inputPort.MealSlotID != null)
        {
            _MealSlot = _PersistenceContext.GetEntities<MealSlot>()
                .Where(ms => ms.MealSlotID == inputPort.MealSlotID
                    && ms.Household.HouseholdID == _Household.HouseholdID)
                .SingleOrDefault();

            if (_MealSlot == null)
            {
                await outputPort.PresentMealSlotNotFoundAsync(inputPort.MealSlotID.Value, cancellationToken);
                return;
            }
        }

        var _Entry = new MealPlanEntry()
        {
            Date = inputPort.Date.Date,
            Household = _Household,
            MealSlot = _MealSlot,
            Recipe = _Recipe,
            Title = _Title
        };

        _PersistenceContext.Add(_Entry);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealPlanEntryCreatedAsync(_Entry.MealPlanEntryID, cancellationToken);
    }

    #endregion Methods

}
