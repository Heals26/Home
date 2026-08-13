using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;

namespace Home.Application.UseCases.Households.RegisterHousehold;

/// <summary>
/// First-run only: creates the household and its first member in one step, then locks
/// itself — once any user exists, further members are added from inside the app instead.
/// </summary>
internal class RegisterHouseholdInteractor
    : IInteractor<RegisterHouseholdInputPort, IRegisterHouseholdOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RegisterHouseholdInputPort inputPort,
        IRegisterHouseholdOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        if (_PersistenceContext.GetEntities<User>().Any())
        {
            await outputPort.PresentRegistrationClosedAsync(cancellationToken);
        }
        else
        {
            var _PasswordService = serviceFactory.GetService<IPasswordService>();
            var _AuditLogic = serviceFactory.GetService<IAuditLogic<User>>();

            var _Household = new Household() { Name = inputPort.HouseholdName.Trim() };

            var _User = new User()
            {
                Email = inputPort.Email.Trim(),
                FirstName = inputPort.FirstName.Trim(),
                Household = _Household,
                LastName = inputPort.LastName.Trim(),
                MiddleNames = string.Empty
            };

            _AuditLogic.AddAudit(_User);
            _PasswordService.SetPassword(_User, inputPort.Password);

            _PersistenceContext.Add(_Household);
            _PersistenceContext.Add(_User);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentHouseholdRegisteredAsync(_Household.HouseholdID, cancellationToken);
        }
    }

    #endregion Methods

}
