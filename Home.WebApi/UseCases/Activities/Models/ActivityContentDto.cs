namespace Home.WebApi.UseCases.Activities.Models;

public class ActivityContentDto
{

    #region Properties

    public long ActivityContentID { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Sequence { get; set; }

    #endregion Properties

}
