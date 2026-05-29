using AutoMapper;
using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityContents.CreateActivityContent;

namespace Home.WebApi.Presenters.ActivityContents.CreateActivityContent;

public class CreateActivityContentPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateActivityContentOutputPort
{

    #region Methods

    Task ICreateActivityContentOutputPort.PresentActivityContentCreatedAsync(long activityContentID, CancellationToken cancellationToken)
        => this.CreatedAsync(activityContentID, new CreateActivityContentApiResponse() { ActivityContentID = activityContentID }, cancellationToken);

    Task ICreateActivityContentOutputPort.PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Region {activityRegionID} Not Found", cancellationToken);

    #endregion Methods

}
