using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealSlots.CreateMealSlot;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealSlots.CreateMealSlot;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.MealSlots.CreateMealSlot;

/// <summary>
/// Adding a meal to the household's day. The name clash is checked without regard to case, unlike
/// tags, because "Dinner" and "dinner" are the same meal.
/// </summary>
public class CreateMealSlotInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateMealSlotPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name, int sequence)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Recipes = [],
            Sequence = sequence
        };

    private Task HandleAsync(string name, TimeSpan? startsAt = null)
        => new CreateMealSlotInteractor().HandleAsync(
            new CreateMealSlotInputPort(name, startsAt),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheMealToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("  Breakfast  ", new TimeSpan(7, 0, 0));

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<MealSlot>().Single();

        _ = _Stored.Name.Should().Be("Breakfast");
        _ = _Stored.StartsAt.Should().Be(new TimeSpan(7, 0, 0));
        _ = this.Stored<MealSlot>().Count(ms => ms.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsANewMealAtTheEndOfTheDay()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Breakfast", 0), BuildSlot(111, this.Ours, "Lunch", 1));

        await this.HandleAsync("Dinner");

        _ = this.Stored<MealSlot>().Single(ms => ms.Name == "Dinner").Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_OnAHouseholdWithNoMealsStartsAtZero()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Breakfast");

        _ = this.Stored<MealSlot>().Single().Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_AllowsAMealWithNoTimeBecauseAHouseholdNeedNotSayWhenItEats()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Supper");

        _ = this.Stored<MealSlot>().Single().StartsAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdAlreadyHasThatMealInAnyCase_Refuses()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 0));

        await this.HandleAsync("dinner");

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<MealSlot>().Should().ContainSingle("Dinner and dinner are the same meal");
    }

    [Fact]
    public async Task HandleAsync_AllowsAMealNameAnotherHouseholdUses()
    {
        _ = this.Database.Seed(BuildSlot(910, this.Theirs, "Dinner", 0));

        await this.HandleAsync("Dinner");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<MealSlot>().Should().HaveCount(2);
    }

    #endregion Methods

}
