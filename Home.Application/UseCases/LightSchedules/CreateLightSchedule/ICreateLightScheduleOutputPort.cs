using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

public interface ICreateLightScheduleOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken);
    Task PresentLightScheduleCreatedAsync(long lightScheduleID, CancellationToken cancellationToken);

    #endregion Methods

}
