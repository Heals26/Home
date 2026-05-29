using AutoMapper;
using Home.Application.UseCases.ActivityRegions.GetActivityRegions;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegions;

namespace Home.WebApi.Presenters.ActivityRegions.GetActivityRegions;

public class GetActivityRegionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityRegionsOutputPort
{

    #region Methods

    Task IGetActivityRegionsOutputPort.PresentActivityRegionsAsync(IEnumerable<ActivityRegion> activityRegions, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityRegionsApiResponse()
        {
            Regions = [.. activityRegions.OrderBy(r => r.Sequence).Select(r => new GetActivityRegionDto()
            {
                ActivityRegionID = r.ActivityRegionID,
                Region = r.Region?.Name,
                Sequence = r.Sequence
            })]
        }, cancellationToken);

    Task IGetActivityRegionsOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    #endregion Methods

}
