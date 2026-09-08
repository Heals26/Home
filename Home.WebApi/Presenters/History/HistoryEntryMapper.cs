using Home.Application.UseCases.History.Models;
using Home.Domain.Entities;
using Home.WebApi.UseCases.History.Models;

namespace Home.WebApi.Presenters.History;

/// <summary>
/// Turns an audit row into a feed entry. Rows written before 8 Sep 2026 have no summary, only the
/// technical record, so a bland one is made from its first word rather than showing property dumps
/// to a family.
/// </summary>
public static class HistoryEntryMapper
{

    #region Methods

    public static HistoryEntryDto ToDto(Audit audit)
        => new()
        {
            AuditID = audit.AuditID,
            Category = HistoryCategories.Of(audit.Entity) ?? HistoryCategory.Chores,
            EntityID = audit.EntityID,
            ResourceTypeValue = audit.Entity.Value,
            Summary = audit.Summary ?? LegacySummary(audit),
            WhenUTC = audit.ModifiedDateUTC,
            Who = string.IsNullOrWhiteSpace(audit.UserName) ? "Someone" : audit.UserName
        };

    private static string LegacySummary(Audit audit)
    {
        var _Thing = audit.Entity.Name switch
        {
            "Activity" => "a chore",
            "Recipe" => "a recipe",
            "ShoppingCart" => "a shopping list",
            "User" => "a member",
            _ => "something"
        };

        if (audit.Content.StartsWith("Add:", StringComparison.Ordinal))
            return $"added {_Thing}";

        if (audit.Content.StartsWith("Deleted:", StringComparison.Ordinal))
            return $"removed {_Thing}";

        return $"changed {_Thing}";
    }

    #endregion Methods

}
