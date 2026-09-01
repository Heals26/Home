namespace Home.Domain.Entities;

public class Household
{

    #region Properties

    public long HouseholdID { get; set; }

    /// <summary>
    /// Decimal degrees, -90 to 90. Null until the household sets a location — used for
    /// sunrise and sunset schedule triggers.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// The household's LIFX API token. Null when lights aren't connected. Write-only through
    /// the API — responses only ever say whether one is stored.
    /// </summary>
    public string? LifxApiToken { get; set; }

    /// <summary>
    /// Decimal degrees, -180 to 180.
    /// </summary>
    public double? Longitude { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<ActivityState> ActivityStates { get; set; } = [];
    public ICollection<CardSection> CardSections { get; set; } = [];
    public ICollection<LightLocation> LightLocations { get; set; } = [];
    public ICollection<LightScene> LightScenes { get; set; } = [];
    public ICollection<MealSlot> MealSlots { get; set; } = [];
    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<ShoppingList> ShoppingLists { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<User> Members { get; set; } = [];

    #endregion Properties

}
