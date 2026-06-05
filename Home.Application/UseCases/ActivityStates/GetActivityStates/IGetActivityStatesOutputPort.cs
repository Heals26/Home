using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.GetActivityStates;

public interface IGetActivityStatesOutputPort
{

    #region Methods

    Task PresentActivityStatesAsync(IEnumerable<ActivityState> activityStates, CancellationToken cancellationToken);

    #endregion Methods

}
