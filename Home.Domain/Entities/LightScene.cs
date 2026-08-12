namespace Home.Domain.Entities;

/// <summary>
/// A named look for a set of lights — "Movie", "Dinner" — captured from however they were set at
/// the time and recalled with one tap.
/// </summary>
public class LightScene
{

    #region Properties

    public long LightSceneID { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Display order on the Lights page.
    /// </summary>
    public int Sequence { get; set; }

    public Household Household { get; set; }
    public ICollection<LightSceneState> States { get; set; }

    #endregion Properties

}
