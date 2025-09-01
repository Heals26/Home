using CleanArchitecture.Mediator;
using Home.Application.UseCases.Users.UpdateUser;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Users;

public class UpdateUserPresenter : OutputPortPresenter, IUpdateUserOutputPort
{

    #region Methods

    Task<ContinuationBehaviour> IUpdateUserOutputPort.PresentUserConflictAsync(string email, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUpdateUserOutputPort.PresentUserNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
