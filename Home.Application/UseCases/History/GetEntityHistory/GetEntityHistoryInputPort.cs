using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.History.GetEntityHistory;

/// <summary>
/// Everything that happened to one thing: a chore, a recipe. <paramref name="ResourceTypeValue"/> is
/// the resource type's value, as the API carries it.
/// </summary>
public record GetEntityHistoryInputPort(long EntityID, long ResourceTypeValue) : IInputPort<IGetEntityHistoryOutputPort>;
