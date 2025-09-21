
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingCartItemsController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateShoppingCartItemApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingCart(
        [FromServices] CreateShoppingCartItemPresenter presenter,
        [FromBody] CreateShoppingCartItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateShoppingCartItemInputPort() { Name = request.Name }, presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
