namespace Home.WebUI.DataAccess.Activities.SetActivityTags;

public class SetActivityTagsWebAppRequest
{

    #region Properties

    /// <summary>
    /// The complete set of labels the card should end up with — anything left out is taken off it.
    /// </summary>
    public List<long> TagIDs { get; set; } = [];

    #endregion Properties

}
