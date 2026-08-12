using Home.WebApi.UseCases.LightSchedules.Models;

namespace Home.WebApi.UseCases.LightSchedules.GetLightSchedules;

public class GetLightSchedulesApiResponse
{

    #region Properties

    /// <summary>
    /// Every schedule in the household, earliest time of day first.
    /// </summary>
    public List<LightScheduleDto> Schedules { get; set; } = [];

    #endregion Properties

}
