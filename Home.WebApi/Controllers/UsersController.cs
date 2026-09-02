using Home.Application.UseCases.Users.CreateUser;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Application.UseCases.Users.GetUser;
using Home.Application.UseCases.Users.GetUsers;
using Home.Application.UseCases.Users.UpdateUser;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Users.CreateUser;
using Home.WebApi.Presenters.Users.DeleteUser;
using Home.WebApi.Presenters.Users.GetUser;
using Home.WebApi.Presenters.Users.GetUsers;
using Home.WebApi.Presenters.Users.UpdateUser;
using Home.WebApi.UseCases.Users.CreateUser;
using Home.WebApi.UseCases.Users.GetUser;
using Home.WebApi.UseCases.Users.GetUsers;
using Home.WebApi.UseCases.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class UsersController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateUserApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(
        [FromServices] CreateUserPresenter presenter,
        [FromBody] CreateUserApiRequest body,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateUserInputPort(body.Email, body.FirstName, body.LastName, body.MiddleNames, body.Password),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpDelete("{userID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(
        [FromServices] DeleteUserPresenter presenter,
        [FromRoute] long userID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteUserInputPort(userID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{userID}")]
    [ProducesResponseType<GetUserApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(
        [FromServices] GetUserPresenter presenter,
        [FromRoute] long userID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetUserInputPort(userID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet]
    [ProducesResponseType<GetUsersApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromServices] GetUsersPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetUsersInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{userID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(
        [FromServices] UpdateUserPresenter presenter,
        [FromRoute] long userID,
        [FromBody] UpdateUserApiRequest body,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateUserInputPort(body.Email, body.FirstName, body.LastName, body.MiddleNames, body.Password, userID),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
