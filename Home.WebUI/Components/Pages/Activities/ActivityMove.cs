using Home.WebUI.DataAccess.Activities.Models;

namespace Home.WebUI.Components.Pages.Activities;

/// <summary>
/// A card and the column it is being moved into. A null StateID puts it back in "Not sorted".
/// </summary>
public record ActivityMove(ActivitySummaryDto Activity, long? StateID);
