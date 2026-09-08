using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardAgenda
{

    #region Constants

    private const int NextDayCount = 3;

    #endregion Constants

    #region Fields

    private readonly List<HomeSegmentedControl<bool>.SegmentOption> m_ScopeOptions =
    [
        new("Everyone", false),
        new("Just me", true)
    ];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Today first, then the days after it. Null while loading.
    /// </summary>
    [Parameter] public List<CalendarDayDto>? Days { get; set; }

    [Parameter] public EventCallback<CalendarItemDto> OnCompleteTask { get; set; }

    /// <summary>
    /// Show only what involves the signed-in member, plus whatever is the whole household's. A
    /// per-device choice: the kitchen tablet shows everyone, a phone shows its owner.
    /// </summary>
    [Parameter] public bool OnlyMine { get; set; }

    [Parameter] public EventCallback<bool> OnlyMineChanged { get; set; }

    /// <summary>
    /// Who this device is signed in as. Null hides the scope switch, since "me" would mean nobody.
    /// </summary>
    [Parameter] public long? SignedInUserID { get; set; }

    [Parameter] public DateOnly Today { get; set; }

    #endregion Properties

    #region Methods

    private static string DotClass(CalendarItemDto item)
        => item.Kind switch
        {
            CalendarItemKind.Meal => "bg-recipes",
            CalendarItemKind.Task => item.IsDone ? "bg-ink-600" : "bg-week",
            CalendarItemKind.Subscribed => "bg-ink-500",
            _ => "bg-calendar"
        };

    private IEnumerable<CalendarDayDto> NextDays()
        => (this.Days ?? []).Where(d => d.Date > this.Today).Take(NextDayCount);

    private List<CalendarItemDto> TodayItems()
        => this.Visible((this.Days ?? []).FirstOrDefault(d => d.Date == this.Today)?.Items ?? []);

    /// <summary>
    /// An item that names nobody is everyone's and stays; one that names people stays only if the
    /// signed-in member is among them.
    /// </summary>
    private List<CalendarItemDto> Visible(List<CalendarItemDto> items)
        => this.OnlyMine && this.SignedInUserID is { } _UserID
            ? [.. items.Where(i => i.PersonUserIDs.Count == 0 || i.PersonUserIDs.Contains(_UserID))]
            : items;

    #endregion Methods

}
