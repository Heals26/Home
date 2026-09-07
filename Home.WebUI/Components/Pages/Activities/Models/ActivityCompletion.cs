using Home.WebUI.DataAccess.Activities.Models;

namespace Home.WebUI.Components.Pages.Activities.Models;

public record ActivityCompletion(ActivitySummaryDto Activity, bool IsComplete);
