using Home.Domain.Entities;

namespace Home.Application.UseCases.MealSlots.GetMealSlots;

public interface IGetMealSlotsOutputPort
{

    #region Methods

    Task PresentMealSlotsAsync(IEnumerable<MealSlot> mealSlots, CancellationToken cancellationToken);

    #endregion Methods

}
