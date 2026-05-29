using AutoMapper;
using Home.Application.UseCases.ActivityContents.GetActivityContents;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityContents.GetActivityContents;

namespace Home.WebApi.Presenters.ActivityContents.GetActivityContents;

public class GetActivityContentsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityContentsOutputPort
{

    #region Methods

    Task IGetActivityContentsOutputPort.PresentActivityContentsAsync(IEnumerable<ActivityContent> activityContents, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityContentsApiResponse(
            [.. activityContents.OrderBy(c => c.Sequence).Select(c => new GetActivityContentDto(
                c.ActivityContentID,
                c.Content,
                c.Sequence))]),
            cancellationToken);

    Task IGetActivityContentsOutputPort.PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Region {activityRegionID} Not Found", cancellationToken);

    #endregion Methods

}
