namespace Home.WebUI.DataAccess.LightScenes.Models;

public class LightSceneDto
{

    #region Properties

    /// <summary>
    /// How many lights the scene sets when recalled.
    /// </summary>
    public int LightCount { get; set; }

    /// <summary>
    /// The ID of the scene.
    /// </summary>
    public long LightSceneID { get; set; }

    /// <summary>
    /// The scene's name, e.g. "Movie".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
