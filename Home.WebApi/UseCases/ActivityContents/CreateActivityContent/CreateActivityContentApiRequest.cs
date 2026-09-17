namespace Home.WebApi.UseCases.ActivityContents.CreateActivityContent;

public class CreateActivityContentApiRequest
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public string Content { get; set; } = string.Empty;

    #endregion Properties

}
