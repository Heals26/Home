using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Tags.UpdateTag;

public class UpdateTagWebAppRequest
{

    #region Properties

    /// <summary>
    /// A hex colour in the form #RRGGBB.
    /// </summary>
    public PropertyChangeTracker<string> Colour { get; set; }

    /// <summary>
    /// The name of the label.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
