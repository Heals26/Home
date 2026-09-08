namespace Home.Domain.Entities;

/// <summary>
/// One occurrence of a repeating event that does not happen. A moved occurrence is one of these
/// plus an ordinary standalone event; the two are deliberately not linked.
/// </summary>
public class CalendarEventException
{

    #region Properties

    public long CalendarEventExceptionID { get; set; }

    /// <summary>
    /// The date the skipped occurrence would have started, in the series' own zone.
    /// </summary>
    public DateOnly OccurrenceDate { get; set; }

    public CalendarEvent CalendarEvent { get; set; } = null!;

    #endregion Properties

}
