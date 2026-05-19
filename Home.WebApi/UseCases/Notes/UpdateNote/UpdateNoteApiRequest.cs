using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Notes.UpdateNote;

public record UpdateNoteApiRequest(PropertyChangeTracker<string> Content);
