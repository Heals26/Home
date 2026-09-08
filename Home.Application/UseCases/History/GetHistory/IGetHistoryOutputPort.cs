using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.Domain.Entities;

namespace Home.Application.UseCases.History.GetHistory;

public interface IGetHistoryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    /// <summary>
    /// The page, newest first. <paramref name="hasMore"/> says whether an older page exists.
    /// </summary>
    Task PresentHistoryAsync(IReadOnlyList<Audit> audits, bool hasMore, CancellationToken cancellationToken);

    #endregion Methods

}
