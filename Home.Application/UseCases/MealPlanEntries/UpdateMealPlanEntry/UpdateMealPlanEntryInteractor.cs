using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

internal class UpdateMealPlanEntryInteractor : IInteractor<UpdateMealPlanEntryInputPort, IUpdateMealPlanEntryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateMealPlanEntryInputPort inputPort,
        IUpdateMealPlanEntryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // The household is reached through the recipe, the same path the entry itself hangs from.
        var _Entry = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.MealPlanEntryID == inputPort.MealPlanEntryID
                && e.Recipe.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Entry == null)
        {
            await outputPort.PresentMealPlanEntryNotFoundAsync(inputPort.MealPlanEntryID, cancellationToken);
        }
        else
        {
            if (inputPort.Date.HasBeenSet)
                _Entry.Date = inputPort.Date.Value.Date;

            // A meal slot the household does not own simply misses, which clears the slot rather
            // than moving the meal onto another family's.
            if (inputPort.MealSlotID.HasBeenSet)
                _Entry.MealSlot = inputPort.MealSlotID.Value.HasValue
                    ? _PersistenceContext.GetEntities<MealSlot>()
                        .SingleOrDefault(s => s.MealSlotID == inputPort.MealSlotID.Value.Value && s.Household.HouseholdID == _Household.HouseholdID)
                    : null;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentMealPlanEntryNoContentAsync(cancellationToken);
        }
    }

    #endregion Methods

}