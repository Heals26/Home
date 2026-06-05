using AutoMapper;
using Home.Application.UseCases.Users.DeleteUser;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Users.DeleteUser;

public class DeleteUserPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteUserOutputPort
{

    #region Methods

    Task IDeleteUserOutputPort.PresentUserDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
