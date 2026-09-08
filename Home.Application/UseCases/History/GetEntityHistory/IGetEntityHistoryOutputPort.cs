using Home.Domain.Entities;

namespace Home.Application.UseCases.History.GetEntityHistory;

public interface IGetEntityHistoryOutputPort
{

    #region Methods

    Task PresentEntityHistoryAsync(IReadOnlyList<Audit> audits, CancellationToken cancellationToken);

    #endregion Methods

}
