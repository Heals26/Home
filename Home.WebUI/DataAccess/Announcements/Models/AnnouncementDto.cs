namespace Home.WebUI.DataAccess.Announcements.Models;

public class AnnouncementDto
{

    #region Properties

    /// <summary>
    /// The ID of the announcement.
    /// </summary>
    public long AnnouncementID { get; set; }

    /// <summary>
    /// The note's text, at most 500 characters.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the note was pinned (UTC).
    /// </summary>
    public DateTime CreatedOnUTC { get; set; }

    #endregion Properties

}
