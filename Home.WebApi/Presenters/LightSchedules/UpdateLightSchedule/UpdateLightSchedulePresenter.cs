using AutoMapper;
using Home.Application.UseCases.LightSchedules.UpdateLightSchedule;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightSchedules.UpdateLightSchedule;

public class UpdateLightSchedulePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateLightScheduleOutputPort
{

    #region Methods

    Task IUpdateLightScheduleOutputPort.PresentLightScheduleNotFoundAsync(long lightScheduleID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Schedule {lightScheduleID} Not Found", cancellationToken);

    Task IUpdateLightScheduleOutputPort.PresentLightScheduleUpdatedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
