namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

public interface IUpdateMealPlanEntryOutputPort
{

    #region Methods

    Task PresentMealPlanEntryNoContentAsync(CancellationToken cancellationToken);
    Task PresentMealPlanEntryNotFoundAsync(long mealPlanEntryID, CancellationToken cancellationToken);

    #endregion Methods

}