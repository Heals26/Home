using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetActivities;

public interface IGetActivitiesOutputPort
{

    #region Methods

    Task PresentActivitiesAsync(IEnumerable<Activity> activities, CancellationToken cancellationToken);

    #endregion Methods

}
