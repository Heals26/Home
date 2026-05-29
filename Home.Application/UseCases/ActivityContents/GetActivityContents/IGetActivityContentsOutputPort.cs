using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContents;

public interface IGetActivityContentsOutputPort
{

    #region Methods

    Task PresentActivityContentsAsync(IEnumerable<ActivityContent> activityContents, CancellationToken cancellationToken);
    Task PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
