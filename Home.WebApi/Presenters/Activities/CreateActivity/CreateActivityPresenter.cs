using AutoMapper;
using Home.Application.UseCases.Activities.CreateActivity;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Activities.CreateActivity;

namespace Home.WebApi.Presenters.Activities.CreateActivity;

public class CreateActivityPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateActivityOutputPort
{

    #region Methods

    Task ICreateActivityOutputPort.PresentActivityCreatedAsync(long activityID, CancellationToken cancellationToken)
        => this.CreatedAsync(activityID, new CreateActivityApiResponse() { ActivityID = activityID }, cancellationToken);

    #endregion Methods

}
