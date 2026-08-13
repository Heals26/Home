using Home.WebUI.DataAccess.LightScenes.Models;

namespace Home.WebUI.DataAccess.LightScenes.GetLightScenes;

public class GetLightScenesWebAppResponse
{

    #region Properties

    /// <summary>
    /// Every saved scene in the household, in display order.
    /// </summary>
    public List<LightSceneDto> Scenes { get; set; } = [];

    #endregion Properties

}
