using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.UseCases.ActivityStates.DeleteActivityState;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityStates.DeleteActivityState;

public class DeleteActivityStatePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteActivityStateOutputPort
{

    #region Methods

    Task IDeleteActivityStateOutputPort.PresentActivityStateDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task<ContinuationBehaviour> IDeleteActivityStateOutputPort.PresentActivityStateInUseAsync(long activityStateID, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IDeleteActivityStateOutputPort.PresentActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity State {activityStateID} Not Found", cancellationToken);

    Task<ContinuationBehaviour> IDeleteActivityStateOutputPort.PresentLastActivityStateAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IDeleteActivityStateOutputPort.PresentTargetActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity State {activityStateID} Not Found", cancellationToken);

    #endregion Methods

}
