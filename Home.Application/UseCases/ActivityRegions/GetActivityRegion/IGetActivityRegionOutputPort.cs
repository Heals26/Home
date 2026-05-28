using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegion;

public interface IGetActivityRegionOutputPort
{

    #region Methods

    Task PresentActivityRegionAsync(ActivityRegion activityRegion, CancellationToken cancellationToken);
    Task PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
