using Home.Application.UseCases.History.Models;

namespace Home.WebApi.UseCases.History.Models;

/// <summary>
/// One thing that happened. <see cref="Summary"/> is already in words ("ticked off 'Bins'");
/// the screen puts <see cref="Who"/> in front of it.
/// </summary>
public class HistoryEntryDto
{

    #region Properties

    public long AuditID { get; set; }
    public HistoryCategory Category { get; set; }

    /// <summary>
    /// The thing it happened to, by its own kind's ID, so a row can link to it.
    /// </summary>
    public long EntityID { get; set; }

    /// <summary>
    /// The resource type's value, for a screen that needs to know what kind of thing it is.
    /// </summary>
    public long ResourceTypeValue { get; set; }

    public string Summary { get; set; } = string.Empty;
    public DateTime WhenUTC { get; set; }

    /// <summary>
    /// The member who did it. "Someone" when the row was written without a person.
    /// </summary>
    public string Who { get; set; } = string.Empty;

    #endregion Properties

}
