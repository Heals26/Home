using AutoMapper;
using Home.Application.UseCases.History.GetEntityHistory;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.History.GetEntityHistory;

namespace Home.WebApi.Presenters.History.GetEntityHistory;

public class GetEntityHistoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetEntityHistoryOutputPort
{

    #region Methods

    Task IGetEntityHistoryOutputPort.PresentEntityHistoryAsync(IReadOnlyList<Audit> audits, CancellationToken cancellationToken)
        => this.OkAsync(new GetEntityHistoryApiResponse()
        {
            Entries = [.. audits.Select(HistoryEntryMapper.ToDto)]
        }, cancellationToken);

    #endregion Methods

}
