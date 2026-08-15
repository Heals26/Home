using AutoMapper;
using Home.Application.UseCases.MealSlots.DeleteMealSlot;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.MealSlots.DeleteMealSlot;

public class DeleteMealSlotPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteMealSlotOutputPort
{

    #region Methods

    Task IDeleteMealSlotOutputPort.PresentMealSlotDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteMealSlotOutputPort.PresentMealSlotInUseAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IDeleteMealSlotOutputPort.PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Slot {mealSlotID} Not Found", cancellationToken);

    #endregion Methods

}
