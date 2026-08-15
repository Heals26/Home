using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealSlots.DeleteMealSlot;

internal class DeleteMealSlotInteractor : IInteractor<DeleteMealSlotInputPort, IDeleteMealSlotOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteMealSlotInputPort inputPort,
        IDeleteMealSlotOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _MealSlot = _PersistenceContext.GetEntities<MealSlot>()
            .Where(ms => ms.MealSlotID == inputPort.MealSlotID
                && ms.Household.HouseholdID == _Household.HouseholdID)
            .Select(ms => new
            {
                MealSlot = ms,
                ms.Recipes
            })
            .SingleOrDefault()
            ?.MealSlot;

        if (_MealSlot == null)
        {
            await outputPort.PresentMealSlotNotFoundAsync(inputPort.MealSlotID, cancellationToken);
            return;
        }

        // Planned meals hold the slot on a restricted foreign key, so the refusal has to happen
        // here rather than as a failed save.
        var _IsPlanned = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Any(e => e.Recipe.Household.HouseholdID == _Household.HouseholdID
                && e.MealSlot != null
                && e.MealSlot.MealSlotID == _MealSlot.MealSlotID);

        if (_IsPlanned)
        {
            await outputPort.PresentMealSlotInUseAsync(inputPort.MealSlotID, cancellationToken);
            return;
        }

        // The recipe links never cascade from this side — a second cascade path is not allowed.
        _PersistenceContext.RemoveRange(_MealSlot.Recipes);
        _PersistenceContext.Remove(_MealSlot);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealSlotDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
