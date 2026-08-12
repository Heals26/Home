using Home.Domain.Entities;

namespace Home.Application.UseCases.LightSchedules.GetLightSchedules;

public interface IGetLightSchedulesOutputPort
{

    #region Methods

    Task PresentLightSchedulesAsync(IReadOnlyList<LightSchedule> schedules, CancellationToken cancellationToken);

    #endregion Methods

}
