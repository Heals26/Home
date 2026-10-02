using Home.Domain.Deletions;
using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

/// <summary>
/// A problem or an idea a member sent about the app, kept with enough of where they were to make
/// sense of it later. It is addressed to whoever supports the app rather than to the household, so
/// it is not written to the history feed.
/// </summary>
public class Feedback : ISoftDeletable
{

    #region Properties

    public long FeedbackID { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedOnUTC { get; set; }

    public DateTime? DeletedOnUTC { get; set; }

    /// <summary>
    /// What the session it was sent from is called. Copied rather than linked, because a session
    /// ends and the feedback should still say where it came from.
    /// </summary>
    public string? DeviceLabel { get; set; }

    public Household Household { get; set; } = null!;

    public FeedbackKind Kind { get; set; }

    /// <summary>
    /// The page the feedback is about. Sent from Settings, that is the page before Settings.
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// The browser window in CSS pixels, which is what the layout had to fit.
    /// </summary>
    public int? ScreenHeight { get; set; }

    public int? ScreenWidth { get; set; }

    /// <summary>
    /// Who sent it. Null once that member has been removed: the feedback stays, without a name.
    /// </summary>
    public User? User { get; set; }

    #endregion Properties

}
