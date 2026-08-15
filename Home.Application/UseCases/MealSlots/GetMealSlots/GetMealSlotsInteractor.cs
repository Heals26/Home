using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealSlots.GetMealSlots;

internal class GetMealSlotsInteractor : IInteractor<GetMealSlotsInputPort, IGetMealSlotsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetMealSlotsInputPort inputPort,
        IGetMealSlotsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _MealSlots = _PersistenceContext.GetEntities<MealSlot>()
            .Where(ms => ms.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(ms => ms.Sequence)
            .ThenBy(ms => ms.Name)
            .ToList();

        await outputPort.PresentMealSlotsAsync(_MealSlots, cancellationToken);
    }

    #endregion Methods

}
