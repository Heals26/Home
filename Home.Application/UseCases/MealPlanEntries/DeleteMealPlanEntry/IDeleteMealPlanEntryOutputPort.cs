namespace Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;

public interface IDeleteMealPlanEntryOutputPort
{

    #region Methods

    Task PresentMealPlanEntryDeletedAsync(CancellationToken cancellationToken);
    Task PresentMealPlanEntryNotFoundAsync(long mealPlanEntryID, CancellationToken cancellationToken);

    #endregion Methods

}
