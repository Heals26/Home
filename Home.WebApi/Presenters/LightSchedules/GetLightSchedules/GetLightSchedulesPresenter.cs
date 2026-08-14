using AutoMapper;
using Home.Application.UseCases.LightSchedules.GetLightSchedules;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.LightSchedules.GetLightSchedules;
using Home.WebApi.UseCases.LightSchedules.Models;

namespace Home.WebApi.Presenters.LightSchedules.GetLightSchedules;

public class GetLightSchedulesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetLightSchedulesOutputPort
{

    #region Methods

    Task IGetLightSchedulesOutputPort.PresentLightSchedulesAsync(IReadOnlyList<LightSchedule> schedules, CancellationToken cancellationToken)
        => this.OkAsync(new GetLightSchedulesApiResponse()
        {
            Schedules = [.. schedules.Select(s => new LightScheduleDto()
            {
                LightScheduleID = s.LightScheduleID,
                Name = s.Name,
                IsEnabled = s.IsEnabled,
                Trigger = s.Trigger,
                TimeOfDay = s.TimeOfDay,
                OffsetMinutes = s.OffsetMinutes,
                DaysOfWeek = s.DaysOfWeek,
                LastRunUTC = s.LastRunUTC,
                LightSceneID = s.Scene.LightSceneID,
                SceneName = s.Scene.Name
            })]
        }, cancellationToken);

    #endregion Methods

}
