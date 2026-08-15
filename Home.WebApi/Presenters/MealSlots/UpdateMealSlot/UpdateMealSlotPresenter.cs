using AutoMapper;
using Home.Application.UseCases.MealSlots.UpdateMealSlot;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.MealSlots.UpdateMealSlot;

public class UpdateMealSlotPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateMealSlotOutputPort
{

    #region Methods

    Task IUpdateMealSlotOutputPort.PresentMealSlotNameConflictAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUpdateMealSlotOutputPort.PresentMealSlotNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateMealSlotOutputPort.PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Slot {mealSlotID} Not Found", cancellationToken);

    #endregion Methods

}
