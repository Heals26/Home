namespace Home.Application.UseCases.History.Models;

/// <summary>
/// How the feed groups what the house records, in family words rather than table names. A person
/// picks which of these they want to see; the default set is the caller's business.
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
