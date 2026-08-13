using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Households.UpdateHouseholdSettings;

public interface IUpdateHouseholdSettingsOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentHouseholdSettingsUpdatedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
