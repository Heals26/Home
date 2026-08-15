namespace Home.WebUI.DataAccess.ActivityStates.CreateActivityState;

public class CreateActivityStateWebAppRequest
{

    #region Properties

    /// <summary>
    /// Whether landing in this column means the activity is finished.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// The name of the column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
