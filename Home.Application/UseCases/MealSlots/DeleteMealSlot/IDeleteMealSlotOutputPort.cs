namespace Home.Application.UseCases.MealSlots.DeleteMealSlot;

public interface IDeleteMealSlotOutputPort
{

    #region Methods

    Task PresentMealSlotDeletedAsync(CancellationToken cancellationToken);
    Task PresentMealSlotInUseAsync(long mealSlotID, CancellationToken cancellationToken);
    Task PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken);

    #endregion Methods

}
