using Home.WebApi.UseCases.Announcements.Models;

namespace Home.WebApi.UseCases.Announcements.GetAnnouncements;

public class GetAnnouncementsApiResponse
{

    #region Properties

    public ICollection<AnnouncementDto> Announcements { get; set; } = [];

    #endregion Properties

}
