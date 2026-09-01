using Home.WebUI.DataAccess.Activities.Models;

namespace Home.WebUI.Components.Pages.Activities;

/// <summary>
/// Two cards in the same column whose places are being swapped. Carrying the neighbour rather than
/// a target position keeps the page from having to re-derive the column the board just looked at.
/// </summary>
/// <param name="Activity">The card being moved.</param>
/// <param name="Neighbour">The card it is changing places with.</param>
public record ActivityReorder(ActivitySummaryDto Activity, ActivitySummaryDto Neighbour);
