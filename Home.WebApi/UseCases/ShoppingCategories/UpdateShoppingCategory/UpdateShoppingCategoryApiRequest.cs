using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingCategories.UpdateShoppingCategory;

public record UpdateShoppingCategoryApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence);
