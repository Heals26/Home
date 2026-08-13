using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Households.RegisterHousehold;

public interface IRegisterHouseholdOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentHouseholdRegisteredAsync(long householdID, CancellationToken cancellationToken);
    Task PresentRegistrationClosedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
