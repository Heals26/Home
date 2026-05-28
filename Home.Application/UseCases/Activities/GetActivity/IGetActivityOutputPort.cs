using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetActivity;

public interface IGetActivityOutputPort
{

    #region Methods

    Task PresentActivityAsync(Activity activity, CancellationToken cancellationToken);
    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
