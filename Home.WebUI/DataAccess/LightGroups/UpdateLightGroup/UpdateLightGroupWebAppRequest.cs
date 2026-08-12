using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.LightGroups.UpdateLightGroup;

public class UpdateLightGroupWebAppRequest
{

    #region Properties

    /// <summary>
    /// The group's name.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Display order on the Lights page.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    #endregion Properties

}
