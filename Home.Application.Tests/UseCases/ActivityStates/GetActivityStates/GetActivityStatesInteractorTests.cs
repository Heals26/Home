using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityStates.GetActivityStates;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ActivityStates.GetActivityStates;
using Home.WebApi.UseCases.ActivityStates.GetActivityStates;

namespace Home.Application.Tests.UseCases.ActivityStates.GetActivityStates;

/// <summary>
/// The board's columns. These were a global lookup until 15 Aug, so household scoping is the
/// property most worth pinning — one family renaming "Doing" must not rename it for everyone.
/// </summary>
public class GetActivityStatesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivityStatesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ActivityState BuildColumn(long activityStateID, Household household, string name, int sequence, bool isComplete = false)
        => new()
        {
            ActivityStateID = activityStateID,
            Household = household,
            IsComplete = isComplete,
            Name = name,
            Sequence = sequence
        };

    private Task HandleAsync()
        => new GetActivityStatesInteractor().HandleAsync(
            new GetActivityStatesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOurColumnsLeftToRightAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildColumn(122, this.Ours, "Done", 3, isComplete: true),
            BuildColumn(120, this.Ours, "To do", 1),
            BuildColumn(121, this.Ours, "Doing", 2),
            BuildColumn(920, this.Theirs, "Their column", 1));

        await this.HandleAsync();

        var _States = Ok<GetActivityStatesApiResponse>(this.m_Presenter).States;

        _ = _States.Select(s => s.Name).Should().Equal(
            ["To do", "Doing", "Done"],
            "columns are household-scoped and read left to right by sequence");
        _ = _States.Single(s => s.Name == "Done").IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTwoColumnsShareASequence_BreaksTheTieOnIDSoTheBoardDoesNotReshuffle()
    {
        _ = this.Database.Seed(
            BuildColumn(121, this.Ours, "Second", 1),
            BuildColumn(120, this.Ours, "First", 1));

        await this.HandleAsync();

        _ = Ok<GetActivityStatesApiResponse>(this.m_Presenter).States
            .Select(s => s.ActivityStateID).Should().Equal(120, 121);
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdHasNoColumns_PresentsAnEmptyList()
    {
        _ = this.Database.Seed(BuildColumn(920, this.Theirs, "Their column", 1));

        await this.HandleAsync();

        _ = Ok<GetActivityStatesApiResponse>(this.m_Presenter).States.Should().BeEmpty();
    }

    #endregion Methods

}
