namespace Home.Application.UseCases.LightSchedules.UpdateLightSchedule;

public interface IUpdateLightScheduleOutputPort
{

    #region Methods

    Task PresentLightScheduleNotFoundAsync(long lightScheduleID, CancellationToken cancellationToken);
    Task PresentLightScheduleUpdatedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
