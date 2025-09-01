namespace Home.Domain.Entities;


public class ActivityContent
{

    #region Properties

    public long ActivityContentID { get; set; }
    public string Content { get; set; }
    public int Sequence { get; set; }

    public ActivityRegion Region { get; set; }

    #endregion Properties

}
