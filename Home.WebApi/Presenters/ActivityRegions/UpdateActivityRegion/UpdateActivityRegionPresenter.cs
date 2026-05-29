using AutoMapper;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityRegions.UpdateActivityRegion;

public class UpdateActivityRegionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateActivityRegionOutputPort
{

    #region Methods

    Task IUpdateActivityRegionOutputPort.PresentActivityRegionNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateActivityRegionOutputPort.PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Region {activityRegionID} Not Found", cancellationToken);

    #endregion Methods

}
