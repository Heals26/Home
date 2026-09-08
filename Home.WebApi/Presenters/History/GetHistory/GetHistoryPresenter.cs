using AutoMapper;
using Home.Application.UseCases.History.GetHistory;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.History.GetHistory;

namespace Home.WebApi.Presenters.History.GetHistory;

public class GetHistoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetHistoryOutputPort
{

    #region Methods

    Task IGetHistoryOutputPort.PresentHistoryAsync(IReadOnlyList<Audit> audits, bool hasMore, CancellationToken cancellationToken)
        => this.OkAsync(new GetHistoryApiResponse()
        {
            Entries = [.. audits.Select(HistoryEntryMapper.ToDto)],
            HasMore = hasMore
        }, cancellationToken);

    #endregion Methods

}
