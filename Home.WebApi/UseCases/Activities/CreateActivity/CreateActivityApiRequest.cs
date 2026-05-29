namespace Home.WebApi.UseCases.Activities.CreateActivity;

public class CreateActivityApiRequest
{

    #region Properties

    public string Title { get; set; }
    public DateTime? DueDateUTC { get; set; }
    public long? StateID { get; set; }
    public long? StatusID { get; set; }
    public long? UserID { get; set; }

    #endregion Properties

}
