using Home.WebUI.DataAccess.Support.CreateFeedback;
using Home.WebUI.DataAccess.Support.Models;

namespace Home.WebUI.Infrastructure.Services.Support;

public interface IFeedbackLogic
{

    #region Methods

    /// <summary>
    /// What someone wrote, with the page it is about and the size of their browser window. Sent
    /// from Settings, the page is the last one they were on before Settings.
    /// </summary>
    Task<CreateFeedbackWebAppRequest> BuildRequestAsync(FeedbackKind kind, string content, bool fromSettings, CancellationToken cancellationToken);

    #endregion Methods

}
