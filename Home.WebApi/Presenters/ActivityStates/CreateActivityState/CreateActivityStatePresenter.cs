using AutoMapper;
using Home.Application.UseCases.ActivityStates.CreateActivityState;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityStates.CreateActivityState;

namespace Home.WebApi.Presenters.ActivityStates.CreateActivityState;

public class CreateActivityStatePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateActivityStateOutputPort
{

    #region Methods

    Task ICreateActivityStateOutputPort.PresentActivityStateCreatedAsync(long activityStateID, CancellationToken cancellationToken)
        => this.CreatedAsync(activityStateID, new CreateActivityStateApiResponse() { ActivityStateID = activityStateID }, cancellationToken);

    #endregion Methods

}
