namespace Home.WebApi.UseCases.Announcements.Models;

public class AnnouncementDto
{

    #region Properties

    public long AnnouncementID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOnUTC { get; set; }

    #endregion Properties

}
