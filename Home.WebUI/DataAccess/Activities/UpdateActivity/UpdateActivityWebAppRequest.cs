using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Activities.UpdateActivity;

public class UpdateActivityWebAppRequest
{

    #region Properties

    /// <summary>
    /// When the activity was completed (UTC).
    /// </summary>
    public PropertyChangeTracker<DateTime?> CompletedDateUTC { get; set; }

    /// <summary>
    /// When the activity is due (UTC).
    /// </summary>
    public PropertyChangeTracker<DateTime?> DueDateUTC { get; set; }

    /// <summary>
    /// The time of day the activity is due, or null when only the day matters.
    /// </summary>
    public PropertyChangeTracker<TimeSpan?> DueTime { get; set; }

    /// <summary>
    /// The ID of the workflow state (kanban column).
    /// </summary>
    public PropertyChangeTracker<long?> StateID { get; set; }

    /// <summary>
    /// The ID of the status.
    /// </summary>

    /// <summary>
    /// The title of the activity.
    /// </summary>
    public PropertyChangeTracker<string> Title { get; set; }

    /// <summary>
    /// The ID of the assigned user.
    /// </summary>
    public PropertyChangeTracker<long?> UserID { get; set; }

    #endregion Properties

}
