using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ActivityContents.UpdateActivityContent;

public class UpdateActivityContentWebAppRequest
{

    #region Properties

    /// <summary>
    /// The text of the field.
    /// </summary>
    public PropertyChangeTracker<string> Content { get; set; }

    /// <summary>
    /// The order of the field within its group.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    #endregion Properties

}
