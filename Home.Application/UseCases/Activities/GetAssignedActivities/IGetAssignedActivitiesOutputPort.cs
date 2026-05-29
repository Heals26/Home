using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetAssignedActivities;

public interface IGetAssignedActivitiesOutputPort
{

    #region Methods

    Task PresentAssignedActivitiesAsync(IEnumerable<Activity> activities, CancellationToken cancellationToken);

    #endregion Methods

}
