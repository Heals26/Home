using AutoMapper;
using Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.MealPlanEntries.UpdateMealPlanEntry;

public class UpdateMealPlanEntryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateMealPlanEntryOutputPort
{

    #region Methods

    Task IUpdateMealPlanEntryOutputPort.PresentMealPlanEntryNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateMealPlanEntryOutputPort.PresentMealPlanEntryNotFoundAsync(long mealPlanEntryID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Plan Entry {mealPlanEntryID} Not Found", cancellationToken);

    #endregion Methods

}