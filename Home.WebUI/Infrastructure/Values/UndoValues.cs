namespace Home.WebUI.Infrastructure.Values;

public static class UndoValues
{

    #region Fields

    /// <summary>
    /// How long the bar says an undo was turned down before it goes.
    /// </summary>
    public static readonly TimeSpan RefusalShownFor = TimeSpan.FromSeconds(4);

    /// <summary>
    /// How long Undo stays on offer. The API honours it for longer, so an Undo tapped at the last
    /// moment over a slow connection still goes through.
    /// </summary>
    public static readonly TimeSpan ShownFor = TimeSpan.FromSeconds(10);

    #endregion Fields

}
