using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ActivityStates.UpdateActivityState;

public class UpdateActivityStateWebAppRequest
{

    #region Properties

    /// <summary>
    /// Whether landing in this column means the activity is finished.
    /// </summary>
    public PropertyChangeTracker<bool> IsComplete { get; set; }

    /// <summary>
    /// The name of the column.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// The left-to-right order of the column on the board.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    #endregion Properties

}
