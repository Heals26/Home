namespace Home.Application.UseCases.LightSchedules.DeleteLightSchedule;

public interface IDeleteLightScheduleOutputPort
{

    #region Methods

    Task PresentLightScheduleDeletedAsync(CancellationToken cancellationToken);
    Task PresentLightScheduleNotFoundAsync(long lightScheduleID, CancellationToken cancellationToken);

    #endregion Methods

}
