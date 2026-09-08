namespace Home.Application.UseCases.Calendar.Models;

/// <summary>
/// One viewer-local day and everything on it, all-day items first, then by time.
/// </summary>
public record CalendarDay(DateOnly Date, IReadOnlyList<CalendarItem> Items);
