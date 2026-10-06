using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Calendar.Models;
using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.DataAccess.Tags.Models;
using Home.WebUI.Infrastructure.Features;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Features;

public partial class FeatureSampleCard
{

    #region Properties

    [Parameter, EditorRequired] public FeatureSample Sample { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// A line already in the trolley, so the tick the aisle row is about can be seen.
    /// </summary>
    private ShoppingListItemDto AisleLine()
        => new()
        {
            Amount = 2,
            Cost = 4.00m,
            InBasket = true,
            Name = "Milk",
            ShoppingListItemID = 2,
            UnitAbbreviation = "L"
        };

    /// <summary>
    /// A chore the way the board draws one, with somebody's name on it and a label.
    /// </summary>
    private ActivitySummaryDto Chore()
        => new()
        {
            ActivityID = 1,
            AssignedTo = "Jess",
            DueDateUTC = this.Today(),
            State = "This week",
            Tags = [new TagDto() { Colour = "#a3b18a", Name = "House", TagID = 1 }],
            Title = "Put the bins out"
        };

    private CalendarItemDto Event()
    {
        var _Day = DateOnly.FromDateTime(this.Today());

        return new()
        {
            Date = _Day,
            EndDate = _Day,
            EndsAt = new TimeOnly(10, 30),
            ID = 1,
            Kind = CalendarItemKind.Event,
            People = ["Jess"],
            StartDate = _Day,
            StartsAt = new TimeOnly(9, 30),
            Title = "Netball"
        };
    }

    /// <summary>
    /// Which screen the piece came from, so nobody reads a sample as the screen itself.
    /// </summary>
    private string Label()
        => this.Sample switch
        {
            FeatureSample.AisleRow => "Shopping mode",
            FeatureSample.CalendarChip => "Calendar",
            FeatureSample.ChoreCard => "The week",
            FeatureSample.MealCell => "Meal plan",
            FeatureSample.ShoppingLine => "Shopping list",
            _ => string.Empty
        };

    private IEnumerable<MealPlanEntryDto> PlannedMeals()
        => [new MealPlanEntryDto() { Date = this.Today(), MealPlanEntryID = 1, MealSlotName = "Dinner", Name = "Moroccan lamb" }];

    /// <summary>
    /// A line that costs more than it usually does, which is the whole point of showing it.
    /// </summary>
    private ShoppingListItemDto ShoppingLine()
        => new()
        {
            Amount = 500,
            Cost = 9.50m,
            IsDearerThanUsual = true,
            Name = "Olive oil",
            ShoppingListItemID = 1,
            UnitAbbreviation = "ml",
            UsualCost = 7.00m
        };

    private DateTime Today()
        => this.TimeProvider.GetLocalNow().Date;

    #endregion Methods

}
