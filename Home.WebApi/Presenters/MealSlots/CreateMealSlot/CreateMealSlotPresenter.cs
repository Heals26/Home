using AutoMapper;
using Home.Application.UseCases.MealSlots.CreateMealSlot;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.MealSlots.CreateMealSlot;

namespace Home.WebApi.Presenters.MealSlots.CreateMealSlot;

public class CreateMealSlotPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateMealSlotOutputPort
{

    #region Methods

    Task ICreateMealSlotOutputPort.PresentMealSlotCreatedAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.CreatedAsync(mealSlotID, new CreateMealSlotApiResponse() { MealSlotID = mealSlotID }, cancellationToken);

    Task ICreateMealSlotOutputPort.PresentMealSlotNameConflictAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    #endregion Methods

}
