using Home.Domain.Deletions;

namespace Home.Domain.Entities;

/// <summary>
/// A household member who is on an event. Display and filtering only; the household owns the
/// event whoever is on it.
/// </summary>
public class CalendarEventMember : ISoftDeletable
{

    #region Properties

    public long CalendarEventID { get; set; }
    public long UserID { get; set; }

    public CalendarEvent CalendarEvent { get; set; } = null!;
    public DateTime? DeletedOnUTC { get; set; }
    public User User { get; set; } = null!;

    #endregion Properties

}
