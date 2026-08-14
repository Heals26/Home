namespace Home.Domain.Entities;

/// <summary>
/// A short note pinned to the family board — "bin night", "Grandma here Saturday".
/// Deliberately anonymous: the board belongs to the household, not a member.
/// </summary>
public class Announcement
{

    #region Properties

    public long AnnouncementID { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedOnUTC { get; set; }

    public Household Household { get; set; } = null!;

    #endregion Properties

}
