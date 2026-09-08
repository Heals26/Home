using Home.WebApi.UseCases.History.Models;

namespace Home.WebApi.UseCases.History.GetEntityHistory;

public class GetEntityHistoryApiResponse
{

    #region Properties

    public List<HistoryEntryDto> Entries { get; set; } = [];

    #endregion Properties

}
