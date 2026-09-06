using AutoMapper;
using Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.MealPlanEntries.CreateMealPlanEntry;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Presenters.MealPlanEntries.CreateMealPlanEntry;

public class CreateMealPlanEntryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateMealPlanEntryOutputPort
{

    #region Methods

    Task ICreateMealPlanEntryOutputPort.PresentMealPlanEntryCreatedAsync(long mealPlanEntryID, CancellationToken cancellationToken)
        => this.CreatedAsync(mealPlanEntryID, new CreateMealPlanEntryApiResponse() { MealPlanEntryID = mealPlanEntryID }, cancellationToken);

    Task ICreateMealPlanEntryOutputPort.PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Slot {mealSlotID} Not Found", cancellationToken);

    Task ICreateMealPlanEntryOutputPort.PresentNothingToPlanAsync(CancellationToken cancellationToken)
        => this.UnprocessableContent(
            new ValidationProblemDetails()
            {
                Detail = "Give the meal a recipe or a name.",
                Status = (int)HttpStatusCode.UnprocessableContent,
                Title = "Nothing to plan.",
                Errors = new Dictionary<string, string[]>()
            },
            cancellationToken);

    Task ICreateMealPlanEntryOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
