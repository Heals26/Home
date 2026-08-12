using Home.WebApi.UseCases.LightScenes.Models;

namespace Home.WebApi.UseCases.LightScenes.GetLightScenes;

public class GetLightScenesApiResponse
{

    #region Properties

    /// <summary>
    /// Every saved scene in the household, in display order.
    /// </summary>
    public List<LightSceneDto> Scenes { get; set; } = [];

    #endregion Properties

}
