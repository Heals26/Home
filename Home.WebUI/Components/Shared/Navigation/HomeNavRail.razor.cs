using Microsoft.AspNetCore.Components.Routing;

namespace Home.WebUI.Components.Shared.Navigation;

public partial class HomeNavRail
{

    #region Records

    private sealed record NavItem(string Href, string Label, string Icon, string ActiveText, NavLinkMatch Match);

    #endregion Records

    #region Fields

    private static readonly NavItem[] m_Items =
    [
        new("/", "Home", "home", "text-ink-50", NavLinkMatch.All),
        new("/calendar", "Calendar", "calendar", "text-calendar", NavLinkMatch.Prefix),
        new("/recipes", "Recipes", "book", "text-recipes", NavLinkMatch.Prefix),
        new("/meal-plan", "Meals", "utensils", "text-recipes", NavLinkMatch.Prefix),
        new("/shopping-lists", "Shopping", "shopping-list", "text-shopping", NavLinkMatch.Prefix),
        new("/activities", "Week", "board", "text-week", NavLinkMatch.Prefix),
        new("/lights", "Lights", "lightbulb", "text-lights", NavLinkMatch.Prefix),
    ];

    #endregion Fields

}
