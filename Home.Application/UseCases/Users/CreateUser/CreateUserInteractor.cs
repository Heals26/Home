using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
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
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _Mapper = serviceFactory.GetService<IMapper>();
        var _PasswordServive = serviceFactory.GetService<IPasswordService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<User>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _User = _Mapper.Map<User>(inputPort);
        _User.Household = _Household;

        _AuditLogic.AddAudit(_User);

        // Email and password arrive together or not at all (the validator holds that line). A
        // member with neither can be assigned things and named on events but never signs in.
        if (string.IsNullOrWhiteSpace(inputPort.Email) || string.IsNullOrWhiteSpace(inputPort.Password))
        {
            _User.Email = null;
            _User.Password = null;
        }
        else
        {
            _User.Email = inputPort.Email.Trim();
            _PasswordServive.SetPassword(_User, inputPort.Password);
        }

        _PersistenceContext.Add(_User);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentUserCreatedAsync(_User.UserID, cancellationToken);
    }

    #endregion Methods

}

