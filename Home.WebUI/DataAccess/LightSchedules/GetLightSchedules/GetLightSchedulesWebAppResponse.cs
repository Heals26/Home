using Home.WebUI.DataAccess.LightSchedules.Models;

namespace Home.WebUI.DataAccess.LightSchedules.GetLightSchedules;

public class GetLightSchedulesWebAppResponse
{

    #region Properties

    /// <summary>
    /// Every schedule in the household, earliest time of day first.
    /// </summary>
    public List<LightScheduleDto> Schedules { get; set; } = [];

    #endregion Properties

}
