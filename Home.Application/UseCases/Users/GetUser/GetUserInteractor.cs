using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.GetUser;

internal class GetUserInteractor : IInteractor<GetUserInputPort, IGetUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetUserInputPort inputPort,
        IGetUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        await outputPort.PresentUserAsync(_PersistenceContext.Find<User>(inputPort.UserID), cancellationToken);
    }

    #endregion Methods

}
