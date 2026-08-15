namespace Home.Domain.Entities;

/// <summary>
/// A household-defined label with a colour, so the family can see at a glance what kind of thing
/// a card is. The colour is stored as a hex string because the palette is chosen by the family at
/// runtime and cannot come from the compiled Tailwind classes.
/// </summary>
public class Tag
{

    #region Properties

    public long TagID { get; set; }

    public ICollection<ActivityTag> Activities { get; set; } = [];

    /// <summary>
    /// A validated #RRGGBB value. Never interpolated into a class name — only into an inline
    /// style, after validation, so a stored value can't inject CSS.
    /// </summary>
    public string Colour { get; set; } = string.Empty;

    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
