using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.GetUsers;

public interface IGetUsersOutputPort
{

    #region Methods

    Task PresentUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken);

    #endregion Methods

}
