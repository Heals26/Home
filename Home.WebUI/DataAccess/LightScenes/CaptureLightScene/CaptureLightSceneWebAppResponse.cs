namespace Home.WebUI.DataAccess.LightScenes.CaptureLightScene;

public class CaptureLightSceneWebAppResponse
{

    #region Properties

    /// <summary>
    /// How many lights were captured into the scene.
    /// </summary>
    public int LightCount { get; set; }

    /// <summary>
    /// The ID of the created scene.
    /// </summary>
    public long LightSceneID { get; set; }

    #endregion Properties

}
