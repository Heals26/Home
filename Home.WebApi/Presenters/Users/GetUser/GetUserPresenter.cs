using AutoMapper;
using Home.Application.UseCases.Users.GetUser;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Users.GetUser;

public class GetUserPresenter(IMapper mapper) : OutputPortPresenter(mapper), IGetUserOutputPort
{

    #region Methods

    Task IGetUserOutputPort.PresentUserAsync(User user, CancellationToken cancellationToken)
        => this.OkAsync(user, cancellationToken);

    Task IGetUserOutputPort.PresentUserNotFoundAsync(long userID, CancellationToken cancellationToken)
        => this.NotFoundAsync("User Not Found", cancellationToken);

    #endregion Methods

}
