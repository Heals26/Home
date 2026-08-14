namespace Home.WebUI.DataAccess.Announcements.CreateAnnouncement;

public class CreateAnnouncementWebAppRequest
{

    #region Properties

    /// <summary>
    /// The note's text, at most 500 characters.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    #endregion Properties

}
