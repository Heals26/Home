using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealSlots.GetMealSlots;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealSlots.GetMealSlots;
using Home.WebApi.UseCases.MealSlots.GetMealSlots;

namespace Home.Application.Tests.UseCases.MealSlots.GetMealSlots;

/// <summary>
/// The household's own names for its meals. One vocabulary drives both the planner's rows and the
/// recipe book's filter, so the order it comes back in is the order of the family's day.
/// </summary>
public class GetMealSlotsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetMealSlotsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name, int sequence, TimeSpan? startsAt = null)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Sequence = sequence,
            StartsAt = startsAt
        };

    private Task HandleAsync()
        => new GetMealSlotsInteractor().HandleAsync(
            new GetMealSlotsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOurSlotsThroughTheDayAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildSlot(112, this.Ours, "Dinner", 3, new TimeSpan(18, 30, 0)),
            BuildSlot(110, this.Ours, "Breakfast", 1, new TimeSpan(7, 0, 0)),
            BuildSlot(111, this.Ours, "Lunch", 2),
            BuildSlot(910, this.Theirs, "Supper", 1));

        await this.HandleAsync();

        var _Slots = Ok<GetMealSlotsApiResponse>(this.m_Presenter).MealSlots;

        _ = _Slots.Select(s => s.Name).Should().Equal(["Breakfast", "Lunch", "Dinner"]);
        _ = _Slots.Single(s => s.Name == "Breakfast").StartsAt.Should().Be(new TimeSpan(7, 0, 0));
        _ = _Slots.Single(s => s.Name == "Lunch").StartsAt.Should().BeNull("a household need not say when it eats");
    }

    [Fact]
    public async Task HandleAsync_WhenTwoSlotsShareASequence_BreaksTheTieOnName()
    {
        _ = this.Database.Seed(
            BuildSlot(111, this.Ours, "Supper", 1),
            BuildSlot(110, this.Ours, "Brunch", 1));

        await this.HandleAsync();

        _ = Ok<GetMealSlotsApiResponse>(this.m_Presenter).MealSlots
            .Select(s => s.Name).Should().Equal(["Brunch", "Supper"]);
    }

    #endregion Methods

}
