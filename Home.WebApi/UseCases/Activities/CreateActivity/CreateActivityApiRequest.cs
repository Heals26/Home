namespace Home.WebApi.UseCases.Activities.CreateActivity;

public class CreateActivityApiRequest
{

    #region Properties

    public string Title { get; set; }
    public DateTime? DueDateUTC { get; set; }

    /// <summary>
    /// Time of day the activity is due, or null when only the day matters.
    /// </summary>
    public TimeSpan? DueTime { get; set; }

    public long? StateID { get; set; }
    public long? UserID { get; set; }

    #endregion Properties

}
