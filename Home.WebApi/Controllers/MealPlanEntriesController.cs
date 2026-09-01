using Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;
using Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;
using Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;
using Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.MealPlanEntries.CreateMealPlanEntry;
using Home.WebApi.Presenters.MealPlanEntries.DeleteMealPlanEntry;
using Home.WebApi.Presenters.MealPlanEntries.GetMealPlanEntries;
using Home.WebApi.Presenters.MealPlanEntries.UpdateMealPlanEntry;
using Home.WebApi.UseCases.MealPlanEntries.CreateMealPlanEntry;
using Home.WebApi.UseCases.MealPlanEntries.GetMealPlanEntries;
using Home.WebApi.UseCases.MealPlanEntries.UpdateMealPlanEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class MealPlanEntriesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateMealPlanEntryApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMealPlanEntry(
        [FromServices] CreateMealPlanEntryPresenter presenter,
        [FromBody] CreateMealPlanEntryApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateMealPlanEntryInputPort(request.Date, request.MealSlotID, request.RecipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{mealPlanEntryID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMealPlanEntry(
        [FromServices] DeleteMealPlanEntryPresenter presenter,
        [FromRoute] long mealPlanEntryID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteMealPlanEntryInputPort(mealPlanEntryID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetMealPlanEntriesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMealPlanEntries(
        [FromServices] GetMealPlanEntriesPresenter presenter,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetMealPlanEntriesInputPort(fromDate, toDate), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{mealPlanEntryID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMealPlanEntry(
        [FromServices] UpdateMealPlanEntryPresenter presenter,
        [FromRoute] long mealPlanEntryID,
        [FromBody] UpdateMealPlanEntryApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateMealPlanEntryInputPort(request.Date, mealPlanEntryID, request.MealSlotID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
