using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;

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
        var _PasswordServive = serviceFactory.GetService<IPasswordService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<User>>();

        var _User = _Mapper.Map<User>(inputPort);

        _AuditLogic.AddAudit(_User);

        _PasswordServive.SetPassword(_User, inputPort.Password);

        _PersistenceContext.Add(_User);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentUserCreatedAsync(_User.UserID, cancellationToken);
    }

    #endregion Methods

}

