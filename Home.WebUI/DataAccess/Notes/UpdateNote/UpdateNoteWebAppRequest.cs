using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Notes.UpdateNote;

public class UpdateNoteWebAppRequest
{

    #region Properties

    /// <summary>
    /// What the note says.
    /// </summary>
    public PropertyChangeTracker<string> Content { get; set; }

    #endregion Properties

}
