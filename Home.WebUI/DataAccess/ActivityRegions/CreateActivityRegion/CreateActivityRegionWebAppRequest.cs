namespace Home.WebUI.DataAccess.ActivityRegions.CreateActivityRegion;

public class CreateActivityRegionWebAppRequest
{

    #region Properties

    /// <summary>
    /// The ID of the activity the group belongs to.
    /// </summary>
    public long ActivityID { get; set; }

    /// <summary>
    /// Which of the fixed groups to add — Description, AcceptanceCriteria or Notes.
    /// </summary>
    public long CardSectionID { get; set; }

    #endregion Properties

}
