using AutoMapper;
using Home.Application.UseCases.ActivityContents.GetActivityContent;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityContents.GetActivityContent;

namespace Home.WebApi.Presenters.ActivityContents.GetActivityContent;

public class GetActivityContentPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityContentOutputPort
{

    #region Methods

    Task IGetActivityContentOutputPort.PresentActivityContentAsync(ActivityContent activityContent, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityContentApiResponse(
            activityContent.ActivityContentID,
            activityContent.Content,
            activityContent.Sequence),
            cancellationToken);

    Task IGetActivityContentOutputPort.PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Content {activityContentID} Not Found", cancellationToken);

    #endregion Methods

}
