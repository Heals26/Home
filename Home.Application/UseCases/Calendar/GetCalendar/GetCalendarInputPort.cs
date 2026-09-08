using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Calendar.GetCalendar;

/// <summary>
/// Everything on the household's calendar between two viewer-local days, inclusive.
/// <paramref name="TimeZoneID"/> is the viewer's IANA zone, which decides which day a timed
/// event lands on.
/// </summary>
public record GetCalendarInputPort(DateOnly FromDate, string TimeZoneID, DateOnly ToDate) : IInputPort<IGetCalendarOutputPort>;
