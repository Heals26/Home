using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.SetActivityCompletion;

public record SetActivityCompletionInputPort(long ActivityID, bool IsComplete) : IInputPort<ISetActivityCompletionOutputPort>;
