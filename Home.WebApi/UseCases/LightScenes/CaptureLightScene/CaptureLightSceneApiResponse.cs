namespace Home.WebApi.UseCases.LightScenes.CaptureLightScene;

public class CaptureLightSceneApiResponse
{

    #region Properties

    /// <summary>
    /// How many lights were captured into the scene.
    /// </summary>
    public int LightCount { get; set; }

    public long LightSceneID { get; set; }

    #endregion Properties

}
