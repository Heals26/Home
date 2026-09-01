namespace Home.WebUI.DataAccess.Activities.CreateActivity;

public class CreateActivityWebAppRequest
{

    #region Properties

    /// <summary>
    /// When the activity is due (UTC).
    /// </summary>
    public DateTime? DueDateUTC { get; set; }

    /// <summary>
    /// The time of day the activity is due, or null when only the day matters.
    /// </summary>
    public TimeSpan? DueTime { get; set; }

    /// <summary>
    /// The ID of the workflow state (kanban column) to place the activity in.
    /// </summary>
    public long? StateID { get; set; }

    /// <summary>
    /// The ID of the status.
    /// </summary>

    /// <summary>
    /// The title of the activity.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user to assign the activity to.
    /// </summary>
    public long? UserID { get; set; }

    #endregion Properties

}
