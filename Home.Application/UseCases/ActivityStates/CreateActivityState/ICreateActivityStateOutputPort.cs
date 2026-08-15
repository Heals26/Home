using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ActivityStates.CreateActivityState;

public interface ICreateActivityStateOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentActivityStateCreatedAsync(long activityStateID, CancellationToken cancellationToken);

    #endregion Methods

}
