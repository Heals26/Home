using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.SetRecipeMealSlots;

/// <summary>
/// Replaces the whole set of meals a recipe suits, so the caller sends what it wants to be true
/// rather than a list of additions and removals.
/// </summary>
internal class SetRecipeMealSlotsInteractor
    : IInteractor<SetRecipeMealSlotsInputPort, ISetRecipeMealSlotsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetRecipeMealSlotsInputPort inputPort,
        ISetRecipeMealSlotsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID
                && r.Household.HouseholdID == _Household.HouseholdID)
            .Select(r => new
            {
                Recipe = r,
                r.MealSlots
            })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _WantedIDs = inputPort.MealSlotIDs.Distinct().ToList();

        var _HouseholdSlotIDs = _PersistenceContext.GetEntities<MealSlot>()
            .Where(ms => ms.Household.HouseholdID == _Household.HouseholdID)
            .Select(ms => ms.MealSlotID)
            .ToList();

        var _UnknownIDs = _WantedIDs.Except(_HouseholdSlotIDs).ToList();

        if (_UnknownIDs.Count > 0)
        {
            await outputPort.PresentMealSlotNotFoundAsync(_UnknownIDs[0], cancellationToken);
            return;
        }

        _PersistenceContext.RemoveRange(_Recipe.MealSlots.Where(rms => !_WantedIDs.Contains(rms.MealSlotID)).ToList());

        foreach (var _MealSlotID in _WantedIDs.Where(id => _Recipe.MealSlots.All(rms => rms.MealSlotID != id)))
            _PersistenceContext.Add(new RecipeMealSlot()
            {
                MealSlotID = _MealSlotID,
                RecipeID = _Recipe.RecipeID
            });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeMealSlotsSetAsync(cancellationToken);
    }

    #endregion Methods

}
