using CleanArchitecture.Mediator;
using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Users.CreateUser;

namespace Home.WebApi.Presenters.Users;

public class CreateUserPresenter : OutputPortPresenter, ICreateUserOutputPort
{

    #region Methods

    Task<ContinuationBehaviour> ICreateUserOutputPort.PresentUserConflictAsync(string email, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task ICreateUserOutputPort.PresentUserCreatedAsync(long userID, CancellationToken cancellationToken)
        => this.CreatedAsync(userID, new CreateUserApiResponse() { UserID = userID }, cancellationToken);

    #endregion Methods

}
