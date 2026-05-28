using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegions;

public interface IGetActivityRegionsOutputPort
{

    #region Methods

    Task PresentActivityRegionsAsync(IEnumerable<ActivityRegion> activityRegions, CancellationToken cancellationToken);
    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
