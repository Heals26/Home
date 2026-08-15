using Home.WebUI.DataAccess.Activities.Models;

namespace Home.WebUI.Components.Pages.Activities;

public record ActivityCompletion(ActivitySummaryDto Activity, bool IsComplete);
