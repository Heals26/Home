using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.GetActivities;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Activities.GetActivities;
using Home.WebApi.UseCases.Activities.GetActivities;

namespace Home.Application.Tests.UseCases.Activities.GetActivities;

/// <summary>
/// The board itself. Its ordering is the reason a family can read it at a glance, and its
/// projection is what puts a column, an owner and a colour on every card.
/// </summary>
public class GetActivitiesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetActivitiesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, DateTime? dueDateUTC = null, TimeSpan? dueTime = null)
        => new()
        {
            ActivityID = activityID,
            DueDateUTC = dueDateUTC,
            DueTime = dueTime,
            Household = household,
            Title = $"Card {activityID}"
        };

    private Task HandleAsync()
        => new GetActivitiesInteractor().HandleAsync(
            new GetActivitiesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOnlyOurHouseholdsCards()
    {
        _ = this.Database.Seed(
            this.BuildCard(101, this.Ours),
            this.BuildCard(901, this.Theirs),
            this.BuildCard(902, this.Theirs));

        await this.HandleAsync();

        _ = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities
            .Select(a => a.ActivityID).Should().Equal([101], "another family's board is not ours to read");
    }

    [Fact]
    public async Task HandleAsync_PutsDatedCardsFirstAllDayBeforeTimedAndUndatedLast()
    {
        _ = this.Database.Seed(
            this.BuildCard(105, this.Ours),
            this.BuildCard(104, this.Ours),
            this.BuildCard(102, this.Ours, new DateTime(2026, 8, 10), new TimeSpan(9, 0, 0)),
            this.BuildCard(101, this.Ours, new DateTime(2026, 8, 10)),
            this.BuildCard(103, this.Ours, new DateTime(2026, 8, 9), new TimeSpan(17, 0, 0)));

        await this.HandleAsync();

        _ = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities
            .Select(a => a.ActivityID).Should().Equal(103, 101, 102, 104, 105);
    }

    [Fact]
    public async Task HandleAsync_BringsBackTheColumnAssigneeAndTagsOnEveryCard()
    {
        var _Card = this.BuildCard(101, this.Ours);
        _Card.State = new ActivityState() { ActivityStateID = 120, Household = this.Ours, Name = "Doing", Sequence = 1 };
        _Card.User = this.Member;
        _Card.Tags =
        [
            new ActivityTag() { Activity = _Card, Tag = new Tag() { TagID = 151, Colour = "#00FF00", Household = this.Ours, Name = "Outside" } },
            new ActivityTag() { Activity = _Card, Tag = new Tag() { TagID = 150, Colour = "#FF0000", Household = this.Ours, Name = "Chores" } }
        ];

        _ = this.Database.Seed(_Card);

        await this.HandleAsync();

        var _Presented = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities.Single();

        _ = _Presented.State.Should().Be("Doing");
        _ = _Presented.StateID.Should().Be(120);
        _ = _Presented.AssignedTo.Should().Be(this.Member.UserName);
        _ = _Presented.Tags.Select(t => t.Name).Should().Equal("Chores", "Outside");
        _ = _Presented.Tags.Select(t => t.Colour).Should().Equal("#FF0000", "#00FF00");
    }

    [Fact]
    public async Task HandleAsync_WhenTheBoardIsEmpty_PresentsAnEmptyBoardRatherThanFailing()
    {
        _ = this.Database.Seed(this.BuildCard(901, this.Theirs));

        await this.HandleAsync();

        _ = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities.Should().BeEmpty();
    }

    #endregion Methods

}
