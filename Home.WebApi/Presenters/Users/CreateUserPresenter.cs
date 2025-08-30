using CleanArchitecture.Mediator;
using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.InterfaceAdapters.Users;

public class CreateUserPresenter : OutputPortPresenter, ICreateUserOutputPort
{

    #region Methods

    Task<ContinuationBehaviour> ICreateUserOutputPort.PresentUserConflictAsync(string email, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task ICreateUserOutputPort.PresentUserCreatedAsync(long userID, CancellationToken cancellationToken)
        => this.CreatedAsync(userID, userID, cancellationToken);

    #endregion Methods

}
