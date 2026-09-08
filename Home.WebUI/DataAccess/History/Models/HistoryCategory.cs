namespace Home.WebUI.DataAccess.History.Models;

/// <summary>
/// How the feed groups what the house records. Mirrors the API's enumeration value for value.
/// </summary>
public enum HistoryCategory
{
    Chores = 0,
    Meals = 1,
    Recipes = 2,
    Calendar = 3,
    Shopping = 4,
    Members = 5,
}
