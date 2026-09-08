namespace Home.WebUI.DataAccess.Calendar.Models;

/// <summary>
/// One thing on one day, already placed on the viewer's local day by the API, so nothing on
/// screen has to think about zones.
/// </summary>
public class CalendarItemDto
{

    #region Properties

    /// <summary>
    /// The day this item is placed on. A multi-day item appears once per day.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The last day of the whole item, for "continues" labels.
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Local end time on this day, or null when all-day or when the item runs past midnight.
    /// </summary>
    public TimeOnly? EndsAt { get; set; }

    public DateTime? EndUTC { get; set; }

    /// <summary>
    /// The event, meal plan entry or activity behind the item, by its own kind's ID.
    /// </summary>
    public long ID { get; set; }

    public bool IsAllDay { get; set; }

    /// <summary>
    /// Tasks only: already ticked off.
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// Subscribed items cannot be changed in Home.
    /// </summary>
    public bool IsReadOnly { get; set; }

    public bool IsRecurring { get; set; }

    public CalendarItemKind Kind { get; set; }

    public string? Location { get; set; }

    /// <summary>
    /// Events only: the series-local date of this occurrence, which a skip is keyed on.
    /// </summary>
    public DateOnly? OccurrenceDate { get; set; }

    public List<string> People { get; set; } = [];

    /// <summary>
    /// Meals only: the recipe to open, or null for an occasion.
    /// </summary>
    public long? RecipeID { get; set; }

    /// <summary>
    /// The first day of the whole item.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Local time to sort and show by. Null means "any time that day".
    /// </summary>
    public TimeOnly? StartsAt { get; set; }

    public DateTime? StartUTC { get; set; }

    /// <summary>
    /// The meal slot, the assignee, or the subscribed calendar's name.
    /// </summary>
    public string? Subtitle { get; set; }

    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
