using AutoMapper;
using Home.Application.UseCases.ActivityStates.UpdateActivityState;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityStates.UpdateActivityState;

public class UpdateActivityStatePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateActivityStateOutputPort
{

    #region Methods

    Task IUpdateActivityStateOutputPort.PresentActivityStateNotFoundAsync(long activityStateID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity State {activityStateID} Not Found", cancellationToken);

    Task IUpdateActivityStateOutputPort.PresentActivityStateUpdatedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
