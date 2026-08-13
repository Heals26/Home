namespace Home.WebApi.UseCases.Households.GetSetupStatus;

public class GetSetupStatusApiResponse
{

    #region Properties

    /// <summary>
    /// True while the database has no users — the login page offers first-run
    /// registration only in that state.
    /// </summary>
    public bool RequiresSetup { get; set; }

    #endregion Properties

}
