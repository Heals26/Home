using Home.WebApi.UseCases.Activities.Models;
using Home.WebApi.UseCases.Tags.Models;

namespace Home.WebApi.UseCases.Activities.GetActivity;

public class GetActivityApiResponse
{

    #region Properties

    public long ActivityID { get; set; }
    public string Title { get; set; }
    public DateTime? DueDateUTC { get; set; }

    /// <summary>
    /// Time of day the activity is due, or null when only the day matters.
    /// </summary>
    public TimeSpan? DueTime { get; set; }

    public DateTime? CompletedDateUTC { get; set; }
    public long? StateID { get; set; }
    public string State { get; set; }
    public long? StatusID { get; set; }
    public string Status { get; set; }
    public long? AssignedToUserID { get; set; }
    public string AssignedTo { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public List<ActivityRegionDto> Regions { get; set; }

    #endregion Properties

}
