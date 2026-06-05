using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.DeleteUser;

internal class DeleteUserInteractor : IInteractor<DeleteUserInputPort, IDeleteUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteUserInputPort input,
        IDeleteUserOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _User = _PersistenceContext.Find<User>(input.UserID);

        if (_User != null)
            _PersistenceContext.Remove(_User);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentUserDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
