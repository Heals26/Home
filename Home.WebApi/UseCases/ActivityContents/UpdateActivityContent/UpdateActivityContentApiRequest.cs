using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ActivityContents.UpdateActivityContent;

public record UpdateActivityContentApiRequest(
    PropertyChangeTracker<string> Content,
    PropertyChangeTracker<int> Sequence);
