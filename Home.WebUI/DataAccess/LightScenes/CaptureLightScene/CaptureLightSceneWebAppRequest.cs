namespace Home.WebUI.DataAccess.LightScenes.CaptureLightScene;

public class CaptureLightSceneWebAppRequest
{

    #region Properties

    /// <summary>
    /// Leave null to capture every light in the household.
    /// </summary>
    public long? LightGroupID { get; set; }

    /// <summary>
    /// The name to save the scene under.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
