using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Undo.UndoAction;

public record UndoActionInputPort(Guid Token)
    : IInputPort<IUndoActionOutputPort>;
