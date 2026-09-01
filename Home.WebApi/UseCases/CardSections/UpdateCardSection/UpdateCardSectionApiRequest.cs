using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.CardSections.UpdateCardSection;

public record UpdateCardSectionApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence);