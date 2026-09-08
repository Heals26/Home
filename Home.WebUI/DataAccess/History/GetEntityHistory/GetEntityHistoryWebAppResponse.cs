using Home.WebUI.DataAccess.History.Models;

namespace Home.WebUI.DataAccess.History.GetEntityHistory;

public class GetEntityHistoryWebAppResponse
{

    #region Properties

    /// <summary>
    /// What happened to the one thing, newest first.
    /// </summary>
    public List<HistoryEntryDto> Entries { get; set; } = [];

    #endregion Properties

}
