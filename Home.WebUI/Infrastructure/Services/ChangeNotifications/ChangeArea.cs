namespace Home.WebUI.Infrastructure.Services.ChangeNotifications;

/// <summary>
/// The slice of household data a change notification is about, so pages only reload
/// what a change could actually have touched.
/// </summary>
public enum ChangeArea
{
    Activities,
    Announcements,
    Lights,
    MealPlan,
    Recipes,
    Settings,
    ShoppingLists,
    Users,
}
