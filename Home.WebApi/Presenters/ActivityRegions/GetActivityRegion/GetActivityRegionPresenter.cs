using AutoMapper;
using Home.Application.UseCases.ActivityRegions.GetActivityRegion;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Activities.Models;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegion;

namespace Home.WebApi.Presenters.ActivityRegions.GetActivityRegion;

public class GetActivityRegionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetActivityRegionOutputPort
{

    #region Methods

    Task IGetActivityRegionOutputPort.PresentActivityRegionAsync(ActivityRegion activityRegion, CancellationToken cancellationToken)
        => this.OkAsync(new GetActivityRegionApiResponse()
        {
            ActivityRegionID = activityRegion.ActivityRegionID,
            Region = activityRegion.Region?.Name,
            Sequence = activityRegion.Sequence,
            Fields = [.. activityRegion.Fields.OrderBy(f => f.Sequence).Select(f => new ActivityContentDto()
            {
                ActivityContentID = f.ActivityContentID,
                Content = f.Content,
                Sequence = f.Sequence
            })]
        }, cancellationToken);

    Task IGetActivityRegionOutputPort.PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Region {activityRegionID} Not Found", cancellationToken);

    #endregion Methods

}
