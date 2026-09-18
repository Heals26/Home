using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingListItems.CreateShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebApi.Presenters.ShoppingListItems.MoveShoppingListItem;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemCategory;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemInBasket;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemSequence;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebApi.UseCases.ShoppingListItems.SetShoppingListItemCategory;
using Home.WebApi.UseCases.ShoppingListItems.SetShoppingListItemInBasket;
using Home.WebApi.UseCases.ShoppingListItems.SetShoppingListItemSequence;
using Home.WebApi.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingListItemsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateShoppingListItemApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingListItem(
        [FromServices] CreateShoppingListItemPresenter presenter,
        [FromBody] CreateShoppingListItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateShoppingListItemInputPort(request.Amount, request.Cost, request.InBasket, request.Name, request.Note, request.ShoppingListID, request.Unit),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

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

    [HttpGet("Suggestions")]
    [ProducesResponseType<GetShoppingListItemSuggestionsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingListItemSuggestions(
        [FromServices] GetShoppingListItemSuggestionsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingListItemSuggestionsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Moves the item onto another list, landing at the end of it. Its own route rather than a
    /// field on the patch, because moving lists also gives the item a new position and the two
    /// belong together.
    /// </summary>
    [HttpPut("{shoppingListItemID}/List/{shoppingListID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MoveShoppingListItem(
        [FromServices] MoveShoppingListItemPresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromRoute] long shoppingListID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new MoveShoppingListItemInputPort(shoppingListID, shoppingListItemID),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpPut("{shoppingListItemID}/Category")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetShoppingListItemCategory(
        [FromServices] SetShoppingListItemCategoryPresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromBody] SetShoppingListItemCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetShoppingListItemCategoryInputPort(request.ShoppingCategoryID, shoppingListItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Ticks the line into the trolley or takes it back out. Its own route rather than a field on
    /// the patch, because during a shop it is what records and takes back a purchase.
    /// </summary>
    [HttpPut("{shoppingListItemID}/Basket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetShoppingListItemInBasket(
        [FromServices] SetShoppingListItemInBasketPresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromBody] SetShoppingListItemInBasketApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetShoppingListItemInBasketInputPort(request.InBasket, shoppingListItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Puts the line at a position in its list, moving whatever it passes. Its own route rather than
    /// a field on the patch, because it is the one write that changes rows the caller never named.
    /// </summary>
    [HttpPut("{shoppingListItemID}/Sequence")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetShoppingListItemSequence(
        [FromServices] SetShoppingListItemSequencePresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromBody] SetShoppingListItemSequenceApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetShoppingListItemSequenceInputPort(request.Sequence, shoppingListItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{shoppingListItemID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingListItem(
        [FromServices] UpdateShoppingListItemPresenter presenter,
        [FromRoute] long shoppingListItemID,
        [FromBody] UpdateShoppingListItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingListItemInputPort(
            request.Amount,
            request.Cost,
            request.Name,
            request.Note,
            shoppingListItemID,
            request.Unit),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
