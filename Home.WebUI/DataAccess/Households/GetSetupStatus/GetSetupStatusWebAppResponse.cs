namespace Home.WebUI.DataAccess.Households.GetSetupStatus;

public class GetSetupStatusWebAppResponse
{

    #region Properties

    /// <summary>
    /// True while the server has no users at all — the login page offers first-run
    /// registration only in that state.
    /// </summary>
    public bool RequiresSetup { get; set; }

    #endregion Properties

}
