using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

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
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<MealPlanEntry>>();

        var _Household = _AuthorisationService.GetHousehold();

        // The household is reached through the recipe, the same path the entry itself hangs from.
        //
        // MealSlot is projected because this slice can clear it, and clearing a navigation that was
        // never loaded is not a change EF can see: the tracker compares null against null, finds
        // nothing, and leaves the foreign key where it was. Taking a meal out of its slot answered
        // 204 and did nothing at all until 4 Sep 2026.
        var _Entry = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.MealPlanEntryID == inputPort.MealPlanEntryID
                && e.Household.HouseholdID == _Household.HouseholdID)
            .Select(e => new
            {
                Entry = e,
                e.MealSlot,
                // The history names the meal, so the recipe has to be loaded too.
                e.Recipe
            })
            .SingleOrDefault()
            ?.Entry;

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

            // Only an occasion has a name of its own. A meal that is a recipe reads its name from
            // the recipe, so a title sent for one would be written and never seen again.
            if (inputPort.Title.HasBeenSet && _Entry.Recipe == null)
                _Entry.Title = inputPort.Title.Value.Trim();

            _AuditLogic.UpdateAudit(_Entry);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentMealPlanEntryNoContentAsync(cancellationToken);
        }
    }

    #endregion Methods

}