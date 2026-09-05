using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityStates.CreateActivityState;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityStates.CreateActivityState;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ActivityStates.CreateActivityState;

/// <summary>
/// Adding a column. It joins the right-hand end of the board, and the position is worked out here
/// rather than sent by the caller.
/// </summary>
public class CreateActivityStateInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateActivityStatePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ActivityState BuildColumn(long activityStateID, Household household, string name, int sequence)
        => new()
        {
            Activities = [],
            ActivityStateID = activityStateID,
            Household = household,
            Name = name,
            Sequence = sequence
        };

    private Task HandleAsync(string name, bool isComplete = false)
        => new CreateActivityStateInteractor().HandleAsync(
            new CreateActivityStateInputPort(name, isComplete),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheColumnToTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Blocked", isComplete: false);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<ActivityState>().Single();

        _ = _Stored.Name.Should().Be("Blocked");
        _ = _Stored.IsComplete.Should().BeFalse();
        _ = this.Stored<ActivityState>().Count(s => s.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsANewColumnOnTheRightHandEnd()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "To do", 0),
            BuildColumn(121, this.Ours, "Doing", 1));

        await this.HandleAsync("Done");

        _ = this.Stored<ActivityState>().Single(s => s.Name == "Done").Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_OnABoardWithNoColumnsStartsAtZero()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("To do");

        _ = this.Stored<ActivityState>().Single().Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_CountsOnlyOurColumnsWhenWorkingOutTheEnd()
    {
        _ = this.Database.Seed(
            BuildColumn(120, this.Ours, "To do", 0),
            BuildColumn(920, this.Theirs, "Theirs", 47));

        await this.HandleAsync("Doing");

        _ = this.Stored<ActivityState>().Single(s => s.Name == "Doing").Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_TrimsTheNameSoAStraySpaceDoesNotBecomeAColumnHeading()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("  Blocked  ");

        _ = this.Stored<ActivityState>().Single().Name.Should().Be("Blocked");
    }

    [Fact]
    public async Task HandleAsync_RemembersThatAColumnMeansFinished()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Done", isComplete: true);

        _ = this.Stored<ActivityState>().Single().IsComplete.Should().BeTrue();
    }

    #endregion Methods

}
