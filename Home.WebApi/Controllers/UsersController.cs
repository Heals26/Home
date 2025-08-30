using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.InterfaceAdapters.Users;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
public class UsersController : BaseController
{

    #region Fields



    #endregion Fields
    #region Methods

    [Version1]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromServices] CreateUserPresenter presenter, [FromBody] object body, CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateUserInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
