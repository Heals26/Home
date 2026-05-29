using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContent;

public interface IGetActivityContentOutputPort
{

    #region Methods

    Task PresentActivityContentAsync(ActivityContent activityContent, CancellationToken cancellationToken);
    Task PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken);

    #endregion Methods

}
