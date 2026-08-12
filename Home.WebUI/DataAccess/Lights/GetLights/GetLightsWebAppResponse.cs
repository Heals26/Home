using Home.WebUI.DataAccess.Lights.Models;

namespace Home.WebUI.DataAccess.Lights.GetLights;

public class GetLightsWebAppResponse
{

    #region Properties

    /// <summary>
    /// Every light on the account, ordered by group then label.
    /// </summary>
    public List<LightDto> Lights { get; set; } = [];

    #endregion Properties

}
