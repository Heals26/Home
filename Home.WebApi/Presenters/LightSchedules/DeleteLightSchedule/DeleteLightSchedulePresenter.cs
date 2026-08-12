using AutoMapper;
using Home.Application.UseCases.LightSchedules.DeleteLightSchedule;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightSchedules.DeleteLightSchedule;

public class DeleteLightSchedulePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteLightScheduleOutputPort
{

    #region Methods

    Task IDeleteLightScheduleOutputPort.PresentLightScheduleDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteLightScheduleOutputPort.PresentLightScheduleNotFoundAsync(long lightScheduleID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Schedule {lightScheduleID} Not Found", cancellationToken);

    #endregion Methods

}
