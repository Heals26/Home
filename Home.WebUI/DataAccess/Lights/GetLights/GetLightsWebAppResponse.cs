using Home.WebUI.DataAccess.Lights.Models;

namespace Home.WebUI.DataAccess.Lights.GetLights;

public class GetLightsWebAppResponse
{

    #region Properties

    /// <summary>
    /// Every group with its lights, in display order, served from Home's own records.
    /// </summary>
    public List<LightGroupDto> Groups { get; set; } = [];

    #endregion Properties

}
