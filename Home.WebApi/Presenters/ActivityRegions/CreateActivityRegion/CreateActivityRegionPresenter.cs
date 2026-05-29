using AutoMapper;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityRegions.CreateActivityRegion;

namespace Home.WebApi.Presenters.ActivityRegions.CreateActivityRegion;

public class CreateActivityRegionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateActivityRegionOutputPort
{

    #region Methods

    Task ICreateActivityRegionOutputPort.PresentActivityRegionCreatedAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.CreatedAsync(activityRegionID, new CreateActivityRegionApiResponse() { ActivityRegionID = activityRegionID }, cancellationToken);

    Task ICreateActivityRegionOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    #endregion Methods

}
