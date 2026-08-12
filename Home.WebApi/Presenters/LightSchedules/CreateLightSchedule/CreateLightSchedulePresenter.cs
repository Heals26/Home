using AutoMapper;
using Home.Application.UseCases.LightSchedules.CreateLightSchedule;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.LightSchedules.CreateLightSchedule;

namespace Home.WebApi.Presenters.LightSchedules.CreateLightSchedule;

public class CreateLightSchedulePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateLightScheduleOutputPort
{

    #region Methods

    Task ICreateLightScheduleOutputPort.PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Scene {lightSceneID} Not Found", cancellationToken);

    Task ICreateLightScheduleOutputPort.PresentLightScheduleCreatedAsync(long lightScheduleID, CancellationToken cancellationToken)
        => this.CreatedAsync(lightScheduleID, new CreateLightScheduleApiResponse() { LightScheduleID = lightScheduleID }, cancellationToken);

    #endregion Methods

}
