using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Households;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;

namespace Home.Application.UseCases.Households.RegisterHousehold;

/// <summary>
/// Creates a household and its first member in one step. Anyone without an account can do this,
/// not only the first person on a fresh install. Everyone else in the household is added from
/// inside the app.
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
        var _PasswordService = serviceFactory.GetService<IPasswordService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<User>>();
        var _HouseholdSetupLogic = serviceFactory.GetService<IHouseholdSetupLogic>();

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

        _HouseholdSetupLogic.SeedDefaults(_Household);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentHouseholdRegisteredAsync(_Household.HouseholdID, cancellationToken);
    }

    #endregion Methods

}
