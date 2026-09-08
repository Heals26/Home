using Home.WebApi.UseCases.History.Models;

namespace Home.WebApi.UseCases.History.GetHistory;

public class GetHistoryApiResponse
{

    #region Properties

    public List<HistoryEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// Whether an older page exists after the last entry here.
    /// </summary>
    public bool HasMore { get; set; }

    #endregion Properties

}
