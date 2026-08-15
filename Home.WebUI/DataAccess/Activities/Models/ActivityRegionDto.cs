namespace Home.WebUI.DataAccess.Activities.Models;

public class ActivityRegionDto
{

    #region Properties

    /// <summary>
    /// The ID of the group.
    /// </summary>
    public long ActivityRegionID { get; set; }

    /// <summary>
    /// The fields inside the group.
    /// </summary>
    public List<ActivityContentDto> Fields { get; set; } = [];

    /// <summary>
    /// Which of the fixed groups this is — Description, AcceptanceCriteria or Notes.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// The order of the group on the card.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
