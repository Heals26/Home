using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ActivityStates.UpdateActivityState;

public interface IUpdateActivityStateOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken);
    Task PresentActivityStateUpdatedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
