using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ActivityContents.CreateActivityContent;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ActivityContents.CreateActivityContent;

/// <summary>
/// Writing a line under a section of a card. The new line goes on the end, counted from the lines
/// already there, which is the reason the query projects them.
/// </summary>
public class CreateActivityContentInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly CreateActivityContentPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, long regionID, params string[] lines)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        var _Region = new ActivityRegion()
        {
            ActivityRegionID = regionID,
            Activity = _Activity,
            CardSection = new CardSection() { CardSectionID = regionID + 1000, Household = household, Name = "Details", Sequence = 1 },
            Sequence = 1
        };

        _Region.Fields =
        [
            .. lines.Select((line, index) => new ActivityContent()
            {
                ActivityContentID = regionID + index + 1,
                Content = line,
                Region = _Region,
                Sequence = index + 1
            })
        ];

        _Activity.Regions = [_Region];

        return _Activity;
    }

    private Task HandleAsync(long activityRegionID, string content)
    {
        var _Services = this.Services(out var _Context);

        return new CreateActivityContentInteractor().HandleAsync(
            new CreateActivityContentInputPort(activityRegionID, content),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WritesTheLineUnderTheSection()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130));

        await this.HandleAsync(130, "Get cleaning broom");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<ActivityContent>().Single().Content.Should().Be("Get cleaning broom");
        _ = this.Stored<ActivityContent>().Count(c => c.Region.ActivityRegionID == 130).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsTheNewLineOnTheEnd()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130, "First", "Second"));

        await this.HandleAsync(130, "Third");

        _ = this.Stored<ActivityContent>().Single(c => c.Content == "Third").Sequence.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheCardChanged()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130));

        await this.HandleAsync(130, "Get cleaning broom");

        this.m_AuditLogic.Verify(
            a => a.UpdateAudit(It.Is<Activity>(x => x.ActivityID == 100)),
            Times.Once,
            "the audit hangs off the card, not the line, so the query has to load the activity too");
    }

    [Fact]
    public async Task HandleAsync_WhenTheSectionBelongsToAnotherHousehold_PresentsNotFoundAndWritesNothing()
    {
        _ = this.Database.Seed(
            this.BuildCard(100, this.Ours, 130),
            this.BuildCard(900, this.Theirs, 930));

        await this.HandleAsync(930, "Written by us");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityContent>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchSectionExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildCard(100, this.Ours, 130));

        await this.HandleAsync(404, "Get cleaning broom");

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
