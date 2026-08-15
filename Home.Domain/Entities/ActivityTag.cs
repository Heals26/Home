namespace Home.Domain.Entities;

public class ActivityTag
{

    #region Properties

    public long ActivityID { get; set; }
    public long TagID { get; set; }

    public Activity Activity { get; set; } = null!;
    public Tag Tag { get; set; } = null!;

    #endregion Properties

}
