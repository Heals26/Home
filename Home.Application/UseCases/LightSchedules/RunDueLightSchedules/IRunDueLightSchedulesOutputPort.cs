namespace Home.Application.UseCases.LightSchedules.RunDueLightSchedules;

public interface IRunDueLightSchedulesOutputPort
{

    #region Methods

    Task PresentSchedulesRunAsync(int fired, int failed, CancellationToken cancellationToken);

    #endregion Methods

}
