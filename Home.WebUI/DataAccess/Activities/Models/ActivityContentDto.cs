namespace Home.WebUI.DataAccess.Activities.Models;

public class ActivityContentDto
{

    #region Properties

    /// <summary>
    /// The ID of the field.
    /// </summary>
    public long ActivityContentID { get; set; }

    /// <summary>
    /// The text of the field.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The order of the field within its group.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
