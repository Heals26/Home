using CleanArchitecture.Mediator;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.GetUser;

public interface IGetUserOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentUserNotFoundAsync(long userID, CancellationToken cancellationToken);
    Task PresentUserAsync(User user, CancellationToken cancellationToken);

    #endregion Methods

}
