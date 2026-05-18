using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingListItems.CreateShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingListItemsController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateShoppingListItemApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingListItem(
        [FromServices] CreateShoppingListItemPresenter presenter,
        [FromBody] CreateShoppingListItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateShoppingListItemInputPort(request.Cost, request.InBasket, request.Name, request.Quantity, request.ShoppingListID, request.Volume, request.Weight),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpDelete("{shoppingListItemID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteShoppingListItem(
        [FromServices] DeleteShoppingListItemPresenter presenter,
        [FromRoute] long shoppingListItemID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteShoppingListItemInputPort(shoppingListItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{shoppingListItemID}")]
    [ProducesResponseType<GetShoppingListItemApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingListItem(
        [FromServices] GetShoppingListItemPresenter presenter,
        [FromRoute] long shoppingListItemID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingListItemInputPort(shoppingListItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{shoppingListItemID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingListItem(
        [FromServices] UpdateShoppingListItemPresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromBody] UpdateShoppingListItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingListItemInputPort(
            request.Cost,
            request.InBasket,
            request.Name,
            request.Quantity,
            shoppingListItemID,
            request.Sequence,
            request.Volume,
            request.Weight),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
