using CleanArchitecture.Mediator;
using Home.Application.UseCases.History.Models;

namespace Home.Application.UseCases.History.GetHistory;

/// <summary>
/// A page of the household's history, newest first. <paramref name="BeforeAuditID"/> is the last
/// row already shown, so the next page follows on; null starts from the top. An empty
/// <paramref name="Categories"/> means everything the feed shows.
/// </summary>
public record GetHistoryInputPort(long? BeforeAuditID, IReadOnlyList<HistoryCategory> Categories, int Take) : IInputPort<IGetHistoryOutputPort>;
