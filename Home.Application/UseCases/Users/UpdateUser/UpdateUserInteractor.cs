using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;

namespace Home.Application.UseCases.Users.UpdateUser;

internal class UpdateUserInteractor : IInteractor<UpdateUserInputPort, IUpdateUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateUserInputPort inputPort,
        IUpdateUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _Mapper = serviceFactory.GetService<IMapper>();
        var _PasswordServive = serviceFactory.GetService<IPasswordService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<User>>();

        var _User = _PersistenceContext.Find<User>(inputPort.UserID);
        _ = _Mapper.Map(inputPort, _User);

        if (inputPort.Password.HasBeenSet)
            _PasswordServive.SetPassword(_User, inputPort.Password);

        _AuditLogic.UpdateAudit(_User);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentUserNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
