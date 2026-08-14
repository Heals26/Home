using Home.WebUI.DataAccess.Announcements.Models;

namespace Home.WebUI.DataAccess.Announcements.GetAnnouncements;

public class GetAnnouncementsWebAppResponse
{

    #region Properties

    /// <summary>
    /// The household's pinned notes, newest first.
    /// </summary>
    public ICollection<AnnouncementDto> Announcements { get; set; } = [];

    #endregion Properties

}
