using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Notes.UpdateNote;

public record UpdateNoteInputPort(
    long NoteID,
    PropertyChangeTracker<string> Content)
    : IInputPort<IUpdateNoteOutputPort>;
