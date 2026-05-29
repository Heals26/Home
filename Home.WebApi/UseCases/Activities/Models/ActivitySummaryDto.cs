namespace Home.WebApi.UseCases.Activities.Models;

public class ActivitySummaryDto
{

    #region Properties

    public long ActivityID { get; set; }
    public string Title { get; set; }
    public DateTime? DueDateUTC { get; set; }
    public DateTime? CompletedDateUTC { get; set; }
    public long? StateID { get; set; }
    public string State { get; set; }
    public long? StatusID { get; set; }
    public string Status { get; set; }
    public long? AssignedToUserID { get; set; }
    public string AssignedTo { get; set; }

    #endregion Properties

}
