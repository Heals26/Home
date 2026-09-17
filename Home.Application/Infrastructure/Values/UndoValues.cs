namespace Home.Application.Infrastructure.Values;

public static class UndoValues
{

    #region Fields

    /// <summary>
    /// How long a device's Undo is honoured. The bar only shows for about ten seconds; the rest is for
    /// a slow connection, so an undo tapped in time is not turned away on arrival.
    /// </summary>
    public static readonly TimeSpan HonouredFor = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How often the rows no undo can reach any more are deleted for good.
    /// </summary>
    public static readonly TimeSpan PurgeInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long a held-back delete waits before it is carried out. Longer than
    /// <see cref="HonouredFor"/>, so no undo that is still honoured can find its rows gone.
    /// </summary>
    public static readonly TimeSpan PurgedAfter = TimeSpan.FromMinutes(2);

    #endregion Fields

}
