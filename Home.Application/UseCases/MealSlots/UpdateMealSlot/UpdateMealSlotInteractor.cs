using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealSlots.UpdateMealSlot;

internal class UpdateMealSlotInteractor : IInteractor<UpdateMealSlotInputPort, IUpdateMealSlotOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateMealSlotInputPort inputPort,
        IUpdateMealSlotOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _HouseholdSlots = _PersistenceContext.GetEntities<MealSlot>()
            .Where(ms => ms.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        var _MealSlot = _HouseholdSlots.SingleOrDefault(ms => ms.MealSlotID == inputPort.MealSlotID);

        if (_MealSlot == null)
        {
            await outputPort.PresentMealSlotNotFoundAsync(inputPort.MealSlotID, cancellationToken);
            return;
        }

        if (inputPort.Name.HasBeenSet)
        {
            var _Name = inputPort.Name.Value.Trim();

            if (_HouseholdSlots.Any(ms => ms.MealSlotID != _MealSlot.MealSlotID
                && string.Equals(ms.Name, _Name, StringComparison.OrdinalIgnoreCase)))
            {
                await outputPort.PresentMealSlotNameConflictAsync(_Name, cancellationToken);
                return;
            }

            _MealSlot.Name = _Name;
        }

        if (inputPort.Sequence.HasBeenSet)
            _MealSlot.Sequence = inputPort.Sequence.Value;

        if (inputPort.StartsAt.HasBeenSet)
            _MealSlot.StartsAt = inputPort.StartsAt.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealSlotNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
