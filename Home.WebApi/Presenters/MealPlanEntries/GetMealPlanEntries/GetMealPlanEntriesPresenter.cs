using AutoMapper;
using Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.MealPlanEntries.GetMealPlanEntries;
using Home.WebApi.UseCases.MealPlanEntries.Models;

namespace Home.WebApi.Presenters.MealPlanEntries.GetMealPlanEntries;

public class GetMealPlanEntriesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetMealPlanEntriesOutputPort
{

    #region Methods

    Task IGetMealPlanEntriesOutputPort.PresentMealPlanEntriesAsync(IEnumerable<MealPlanEntry> mealPlanEntries, CancellationToken cancellationToken)
        => this.OkAsync(new GetMealPlanEntriesApiResponse()
        {
            Entries = [.. mealPlanEntries.Select(e => new MealPlanEntryDto()
            {
                Date = e.Date,
                MealPlanEntryID = e.MealPlanEntryID,
                RecipeID = e.Recipe.RecipeID,
                RecipeName = e.Recipe.Name
            })]
        }, cancellationToken);

    #endregion Methods

}
