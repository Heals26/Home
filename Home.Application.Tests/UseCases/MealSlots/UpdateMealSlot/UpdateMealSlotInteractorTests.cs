using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.MealSlots.UpdateMealSlot;
using Home.Domain.Entities;
using Home.WebApi.Presenters.MealSlots.UpdateMealSlot;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.MealSlots.UpdateMealSlot;

/// <summary>
/// Renaming a meal, moving it through the day, or saying when it happens.
/// </summary>
public class UpdateMealSlotInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateMealSlotPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static MealSlot BuildSlot(long mealSlotID, Household household, string name, int sequence, TimeSpan? startsAt = null)
        => new()
        {
            Household = household,
            MealSlotID = mealSlotID,
            Name = name,
            Recipes = [],
            Sequence = sequence,
            StartsAt = startsAt
        };

    private Task HandleAsync(
        long mealSlotID,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<int> sequence = default,
        PropertyChangeTracker<TimeSpan?> startsAt = default)
        => new UpdateMealSlotInteractor().HandleAsync(
            new UpdateMealSlotInputPort(mealSlotID, name, sequence, startsAt),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheMealAndSavesIt()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 3));

        await this.HandleAsync(110, name: new("  Tea  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<MealSlot>().Single().Name.Should().Be("Tea");
    }

    [Fact]
    public async Task HandleAsync_CanSayWhenAMealHappensAndTakeItBackAgain()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 3, new TimeSpan(18, 30, 0)));

        await this.HandleAsync(110, startsAt: new(null));

        _ = this.Stored<MealSlot>().Single().StartsAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesThePositionAndTimeAlone()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 3, new TimeSpan(18, 30, 0)));

        await this.HandleAsync(110, name: new("Tea"));

        var _Stored = this.Stored<MealSlot>().Single();

        _ = _Stored.Sequence.Should().Be(3);
        _ = _Stored.StartsAt.Should().Be(new TimeSpan(18, 30, 0));
    }

    [Fact]
    public async Task HandleAsync_SavingAMealUnderItsOwnNameIsNotAClash()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 3));

        await this.HandleAsync(110, name: new("Dinner"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherMealAlreadyHasThatNameInAnyCase_RefusesAndChangesNothing()
    {
        _ = this.Database.Seed(BuildSlot(110, this.Ours, "Dinner", 3), BuildSlot(111, this.Ours, "Lunch", 2));

        await this.HandleAsync(111, name: new("dinner"));

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<MealSlot>().Single(ms => ms.MealSlotID == 111).Name.Should().Be("Lunch");
    }

    [Fact]
    public async Task HandleAsync_WhenTheMealBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildSlot(910, this.Theirs, "Theirs", 1));

        await this.HandleAsync(910, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<MealSlot>().Single().Name.Should().Be("Theirs");
    }

    #endregion Methods

}
