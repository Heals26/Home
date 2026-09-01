using Home.WebUI.DataAccess.Tags.Models;

namespace Home.WebUI.DataAccess.Activities.Models;

public class ActivitySummaryDto
{

    #region Properties

    /// <summary>
    /// The ID of the activity.
    /// </summary>
    public long ActivityID { get; set; }

    /// <summary>
    /// The display name of the assigned user.
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// The ID of the assigned user.
    /// </summary>
    public long? AssignedToUserID { get; set; }

    /// <summary>
    /// When the activity was completed (UTC).
    /// </summary>
    public DateTime? CompletedDateUTC { get; set; }

    /// <summary>
    /// When the activity is due (UTC).
    /// </summary>
    public DateTime? DueDateUTC { get; set; }

    /// <summary>
    /// The time of day the activity is due, or null when only the day matters.
    /// </summary>
    public TimeSpan? DueTime { get; set; }

    /// <summary>
    /// The name of the workflow state (kanban column).
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The ID of the workflow state (kanban column).
    /// </summary>
    public long? StateID { get; set; }


    /// <summary>
    /// The ID of the status.
    /// </summary>

    /// <summary>
    /// The labels on the card.
    /// </summary>
    public List<TagDto> Tags { get; set; } = [];

    /// <summary>
    /// The title of the activity.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
