using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityStates.DeleteActivityState;

public interface IDeleteActivityStateOutputPort
{

    #region Methods

    Task PresentActivityStateDeletedAsync(CancellationToken cancellationToken);
    Task<ContinuationBehaviour> PresentActivityStateInUseAsync(long activityStateID, CancellationToken cancellationToken);
    Task PresentActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken);
    Task<ContinuationBehaviour> PresentLastActivityStateAsync(CancellationToken cancellationToken);
    Task PresentTargetActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken);

    #endregion Methods

}
