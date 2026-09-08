using Home.Application.UseCases.Calendar.Models;

namespace Home.WebApi.UseCases.Calendar.Models;

/// <summary>
/// One thing on one viewer-local day. See <see cref="CalendarItem"/> for what each field means.
/// </summary>
public class CalendarItemDto
{

    #region Properties

    public DateOnly Date { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? EndsAt { get; set; }
    public DateTime? EndUTC { get; set; }
    public long ID { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsDone { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsRecurring { get; set; }
    public CalendarItemKind Kind { get; set; }
    public string Location { get; set; }
    public DateOnly? OccurrenceDate { get; set; }
    public List<string> People { get; set; } = [];
    public long? RecipeID { get; set; }
    public DateOnly StartDate { get; set; }
    public TimeOnly? StartsAt { get; set; }
    public DateTime? StartUTC { get; set; }
    public string Subtitle { get; set; }
    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
