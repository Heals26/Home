namespace Home.Domain.Entities;

/// <summary>
/// A named look for a set of lights — "Movie", "Dinner" — captured from however they were set at
/// the time and recalled with one tap.
/// </summary>
public class LightScene
{

    #region Properties

    public long LightSceneID { get; set; }

    /// <summary>
    /// The household's one automatic scene: how the lights looked just before a scene was last
    /// applied, refreshed on every apply so there is always a way back. Never named by a person.
    /// </summary>
    public bool IsPreviousLook { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order on the Lights page.
    /// </summary>
    public int Sequence { get; set; }

    public Household Household { get; set; } = null!;
    public ICollection<LightSceneState> States { get; set; } = [];

    #endregion Properties

}
