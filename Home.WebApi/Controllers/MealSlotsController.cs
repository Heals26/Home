using Home.Application.UseCases.MealSlots.CreateMealSlot;
using Home.Application.UseCases.MealSlots.DeleteMealSlot;
using Home.Application.UseCases.MealSlots.GetMealSlots;
using Home.Application.UseCases.MealSlots.UpdateMealSlot;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.MealSlots.CreateMealSlot;
using Home.WebApi.Presenters.MealSlots.DeleteMealSlot;
using Home.WebApi.Presenters.MealSlots.GetMealSlots;
using Home.WebApi.Presenters.MealSlots.UpdateMealSlot;
using Home.WebApi.UseCases.MealSlots.CreateMealSlot;
using Home.WebApi.UseCases.MealSlots.GetMealSlots;
using Home.WebApi.UseCases.MealSlots.UpdateMealSlot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class MealSlotsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateMealSlotApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMealSlot(
        [FromServices] CreateMealSlotPresenter presenter,
        [FromBody] CreateMealSlotApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateMealSlotInputPort(request.Name, request.StartsAt), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{mealSlotID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMealSlot(
        [FromServices] DeleteMealSlotPresenter presenter,
        [FromRoute] long mealSlotID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteMealSlotInputPort(mealSlotID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetMealSlotsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMealSlots(
        [FromServices] GetMealSlotsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetMealSlotsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{mealSlotID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMealSlot(
        [FromServices] UpdateMealSlotPresenter presenter,
        [FromRoute] long mealSlotID,
        [FromBody] UpdateMealSlotApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateMealSlotInputPort(mealSlotID, request.Name, request.Sequence, request.StartsAt), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
