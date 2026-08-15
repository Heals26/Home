namespace Home.WebUI.DataAccess.Activities.SetActivityCompletion;

public class SetActivityCompletionWebAppRequest
{

    #region Properties

    /// <summary>
    /// Whether the activity is finished. The API decides which column that means.
    /// </summary>
    public bool IsComplete { get; set; }

    #endregion Properties

}
