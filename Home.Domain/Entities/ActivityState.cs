namespace Home.Domain.Entities;

public class ActivityState
{

    #region Properties

    public long ActivityStateID { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Activity> Activities { get; set; } = [];

    #endregion Properties

}
