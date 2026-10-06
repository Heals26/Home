namespace Home.WebUI.Infrastructure.Features;

/// <summary>
/// The piece of the real app shown beside a feature. Each one is drawn by the same component the
/// screen itself uses, with made-up data, so a sample cannot go stale while the screen moves on.
/// </summary>
public enum FeatureSample
{
    None,
    AisleRow,
    CalendarChip,
    ChoreCard,
    MealCell,
    ShoppingLine,
}
