using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealSlots.CreateMealSlot;

internal class CreateMealSlotInteractor : IInteractor<CreateMealSlotInputPort, ICreateMealSlotOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateMealSlotInputPort inputPort,
        ICreateMealSlotOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();
        var _Name = inputPort.Name.Trim();

        var _ExistingSlots = _PersistenceContext.GetEntities<MealSlot>()
            .Where(ms => ms.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        // The database holds a unique index on household and name; catching it here keeps the
        // family a readable answer instead of a failed save.
        if (_ExistingSlots.Any(ms => string.Equals(ms.Name, _Name, StringComparison.OrdinalIgnoreCase)))
        {
            await outputPort.PresentMealSlotNameConflictAsync(_Name, cancellationToken);
            return;
        }

        var _MealSlot = new MealSlot()
        {
            Household = _Household,
            Name = _Name,
            Recipes = [],
            Sequence = _ExistingSlots.Count == 0 ? 0 : _ExistingSlots.Max(ms => ms.Sequence) + 1,
            StartsAt = inputPort.StartsAt
        };

        _PersistenceContext.Add(_MealSlot);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealSlotCreatedAsync(_MealSlot.MealSlotID, cancellationToken);
    }

    #endregion Methods

}
