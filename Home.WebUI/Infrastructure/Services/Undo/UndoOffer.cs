using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Infrastructure.Services.Undo;

/// <summary>
/// What the bar says was just done, and what has to catch up once it is undone.
/// </summary>
/// <param name="Area">Told to every device once the undo goes through, so whatever shows it reloads.</param>
/// <param name="Summary">The past tense of the control that did it, such as "Deleted Pancakes".</param>
public record UndoOffer(ChangeArea Area, string Summary)
{

    #region Properties

    /// <summary>
    /// Puts back anything this device keeps for itself that the undo cannot reach, such as the shop
    /// it left at Done.
    /// </summary>
    public Func<CancellationToken, Task>? OnUndone { get; init; }

    #endregion Properties

}
