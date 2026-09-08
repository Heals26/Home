namespace Home.Application.UseCases.Calendar.Models;

/// <summary>
/// One thing on one day, whatever it came from. The read model behind the calendar and the
/// dashboard; a projection across events, meals and tasks, already placed on the viewer's local
/// day so nothing rendering it has to know about zones.
/// </summary>
public class CalendarItem
{

    #region Properties

    /// <summary>
    /// The viewer-local day this item is placed on. A multi-day item appears once per day.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Last viewer-local day of the whole item, so a screen can say "continues".
    /// </summary>
    public DateOnly EndDate { get; init; }

    /// <summary>
    /// Viewer-local end time on this day, or null when all-day or when the item runs past it.
    /// </summary>
    public TimeOnly? EndsAt { get; init; }

    public DateTime? EndUTC { get; init; }

    /// <summary>
    /// The event, meal plan entry or activity behind the item, by its own kind's ID.
    /// </summary>
    public long ID { get; init; }

    public bool IsAllDay { get; init; }

    /// <summary>
    /// Tasks only: already completed.
    /// </summary>
    public bool IsDone { get; init; }

    /// <summary>
    /// Subscribed items cannot be changed in Home.
    /// </summary>
    public bool IsReadOnly { get; init; }

    public bool IsRecurring { get; init; }

    public CalendarItemKind Kind { get; init; }

    public string? Location { get; init; }

    /// <summary>
    /// Events only: the series-local date of this occurrence, which is what a skip is keyed on.
    /// </summary>
    public DateOnly? OccurrenceDate { get; init; }

    public IReadOnlyList<string> People { get; init; } = [];

    /// <summary>
    /// Meals only: the recipe to open, or null for an occasion.
    /// </summary>
    public long? RecipeID { get; init; }

    /// <summary>
    /// First viewer-local day of the whole item.
    /// </summary>
    public DateOnly StartDate { get; init; }

    /// <summary>
    /// Viewer-local time to sort and show by. Set on a timed event, on a meal whose slot has a
    /// usual time, and on a task with a due time; null means "any time that day".
    /// </summary>
    public TimeOnly? StartsAt { get; init; }

    /// <summary>
    /// Only set for a real instant: a timed event. Meals and tasks carry a time of day at most.
    /// </summary>
    public DateTime? StartUTC { get; init; }

    /// <summary>
    /// The meal slot, the assignee, or the subscribed calendar's name.
    /// </summary>
    public string? Subtitle { get; init; }

    public string Title { get; init; } = string.Empty;

    #endregion Properties

}
