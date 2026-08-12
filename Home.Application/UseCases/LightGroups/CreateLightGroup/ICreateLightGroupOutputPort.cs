using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.LightGroups.CreateLightGroup;

public interface ICreateLightGroupOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentLightGroupCreatedAsync(long lightGroupID, CancellationToken cancellationToken);
    Task PresentNoLocationAsync(CancellationToken cancellationToken);

    #endregion Methods

}
