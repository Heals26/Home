using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Database;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.CreateUser;

internal class CreateUserInteractor : IInteractor<CreateUserInputPort, ICreateUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateUserInputPort inputPort,
        ICreateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _Mapper = serviceFactory.GetService<IMapper>();

        var _User = _Mapper.Map<User>(inputPort);

        _PersistenceContext.Add(_User);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentUserCreatedAsync(_User.UserID, cancellationToken);
    }

    #endregion Methods

}

