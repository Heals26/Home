using Home.WebUI.DataAccess.History.Models;

namespace Home.WebUI.DataAccess.History.GetHistory;

public class GetHistoryWebAppResponse
{

    #region Properties

    /// <summary>
    /// The page, newest first.
    /// </summary>
    public List<HistoryEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// Whether an older page exists after the last entry here.
    /// </summary>
    public bool HasMore { get; set; }

    #endregion Properties

}
