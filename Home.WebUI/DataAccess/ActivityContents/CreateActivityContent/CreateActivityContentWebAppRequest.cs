namespace Home.WebUI.DataAccess.ActivityContents.CreateActivityContent;

public class CreateActivityContentWebAppRequest
{

    #region Properties

    /// <summary>
    /// The ID of the group the field belongs to.
    /// </summary>
    public long ActivityRegionID { get; set; }

    /// <summary>
    /// The text of the field.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    #endregion Properties

}
