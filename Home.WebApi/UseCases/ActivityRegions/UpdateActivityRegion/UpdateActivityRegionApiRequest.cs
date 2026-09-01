using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ActivityRegions.UpdateActivityRegion;

public record UpdateActivityRegionApiRequest(
    PropertyChangeTracker<int> Sequence);
