using AutoMapper;
using Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ActivityRegions.DeleteActivityRegion;

public class DeleteActivityRegionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteActivityRegionOutputPort
{

    #region Methods

    Task IDeleteActivityRegionOutputPort.PresentActivityRegionDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteActivityRegionOutputPort.PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity Region {activityRegionID} Not Found", cancellationToken);

    #endregion Methods

}
