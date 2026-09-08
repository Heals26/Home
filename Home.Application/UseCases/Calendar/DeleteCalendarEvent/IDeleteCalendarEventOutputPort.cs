namespace Home.Application.UseCases.Calendar.DeleteCalendarEvent;

public interface IDeleteCalendarEventOutputPort
{

    #region Methods

    Task PresentCalendarEventDeletedAsync(CancellationToken cancellationToken);
    Task PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken);

    /// <summary>
    /// The event came from a subscribed calendar; removing it here would only last until the next
    /// refresh, so it is refused with directions instead.
    /// </summary>
    Task PresentCalendarEventReadOnlyAsync(CancellationToken cancellationToken);

    #endregion Methods

}
