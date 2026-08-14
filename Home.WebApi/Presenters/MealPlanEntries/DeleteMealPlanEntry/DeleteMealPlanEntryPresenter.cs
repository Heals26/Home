using AutoMapper;
using Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.MealPlanEntries.DeleteMealPlanEntry;

public class DeleteMealPlanEntryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteMealPlanEntryOutputPort
{

    #region Methods

    Task IDeleteMealPlanEntryOutputPort.PresentMealPlanEntryDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteMealPlanEntryOutputPort.PresentMealPlanEntryNotFoundAsync(long mealPlanEntryID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Plan Entry {mealPlanEntryID} Not Found", cancellationToken);

    #endregion Methods

}
