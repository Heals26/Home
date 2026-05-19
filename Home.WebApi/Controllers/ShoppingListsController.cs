using Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;
using Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;
using Home.Application.UseCases.ShoppingLists.CreateShoppingList;
using Home.Application.UseCases.ShoppingLists.DeleteShoppingList;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Application.UseCases.ShoppingLists.GetShoppingLists;
using Home.Application.UseCases.ShoppingLists.UpdateShoppingList;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItems;
using Home.WebApi.Presenters.ShoppingLists.AddRecipeToShoppingList;
using Home.WebApi.Presenters.ShoppingLists.CreateShoppingList;
using Home.WebApi.Presenters.ShoppingLists.DeleteShoppingList;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingList;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingLists;
using Home.WebApi.Presenters.ShoppingLists.UpdateShoppingList;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;
using Home.WebApi.UseCases.ShoppingLists.CreateShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingLists;
using Home.WebApi.UseCases.ShoppingLists.UpdateShoppingList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingListsController : BaseController
{

    #region Methods

    [HttpPost("{shoppingListID}/Recipes/{recipeID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddRecipeToShoppingList(
        [FromServices] AddRecipeToShoppingListPresenter presenter,
        [FromRoute] long shoppingListID,
        [FromRoute] long recipeID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new AddRecipeToShoppingListInputPort(recipeID, shoppingListID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost]
    [ProducesResponseType<CreateShoppingListApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingList(
        [FromServices] CreateShoppingListPresenter presenter,
        [FromBody] CreateShoppingListApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateShoppingListInputPort(request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{shoppingListID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteShoppingList(
        [FromServices] DeleteShoppingListPresenter presenter,
        [FromRoute] long shoppingListID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteShoppingListInputPort(shoppingListID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{shoppingListID}")]
    [ProducesResponseType<GetShoppingListApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingList(
        [FromServices] GetShoppingListPresenter presenter,
        [FromRoute] long shoppingListID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingListInputPort(shoppingListID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{shoppingListID}/Items")]
    [ProducesResponseType<GetShoppingListItemsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingListItems(
        [FromServices] GetShoppingListItemsPresenter presenter,
        [FromRoute] long shoppingListID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingListItemsInputPort(shoppingListID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetShoppingListsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingLists(
        [FromServices] GetShoppingListsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingListsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{shoppingListID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingList(
        [FromServices] UpdateShoppingListPresenter presenter,
        [FromRoute] long shoppingListID,
        [FromBody] UpdateShoppingListApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingListInputPort(request.Name, shoppingListID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
