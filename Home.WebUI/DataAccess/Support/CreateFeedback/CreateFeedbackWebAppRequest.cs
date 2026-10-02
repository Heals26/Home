using Home.WebUI.DataAccess.Support.Models;

namespace Home.WebUI.DataAccess.Support.CreateFeedback;

public class CreateFeedbackWebAppRequest
{

    #region Properties

    /// <summary>
    /// What the person wrote, at most 2,000 characters.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether it is a problem or an idea.
    /// </summary>
    public FeedbackKind Kind { get; set; }

    /// <summary>
    /// The page the feedback is about, relative to the app's base address.
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// The height of the browser window in CSS pixels, when the browser said.
    /// </summary>
    public int? ScreenHeight { get; set; }

    /// <summary>
    /// The width of the browser window in CSS pixels, when the browser said.
    /// </summary>
    public int? ScreenWidth { get; set; }

    #endregion Properties

}
