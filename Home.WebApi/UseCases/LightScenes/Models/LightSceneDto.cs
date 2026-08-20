namespace Home.WebApi.UseCases.LightScenes.Models;

public class LightSceneDto
{

    #region Properties

    /// <summary>
    /// Whether this is the household's automatic "how it looked before" scene
    /// </summary>
    public bool IsPreviousLook { get; set; }

    /// <summary>
    /// How many lights the scene sets when recalled.
    /// </summary>
    public int LightCount { get; set; }

    public long LightSceneID { get; set; }

    /// <summary>
    /// The scene's name, e.g. "Movie".
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Display order.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
