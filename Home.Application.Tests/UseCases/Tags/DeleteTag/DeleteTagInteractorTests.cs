using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Tags.DeleteTag;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Tags.DeleteTag;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Tags.DeleteTag;

/// <summary>
/// Removing a label. The join to activities has no cascade on the tag side, so those rows have to
/// go first or the database rejects the delete. That clearing is the interesting part.
/// </summary>
public class DeleteTagInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteTagPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Tag BuildTag(long tagID, Household household, string name)
        => new()
        {
            Activities = [],
            Colour = "#FF0000",
            Household = household,
            Name = name,
            TagID = tagID
        };

    private Activity BuildCardTagged(long activityID, Household household, params Tag[] tags)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        _Activity.Tags = [.. tags.Select(t => new ActivityTag() { Activity = _Activity, Tag = t })];

        return _Activity;
    }

    private Task HandleAsync(long tagID)
        => new DeleteTagInteractor().HandleAsync(
            new DeleteTagInputPort(tagID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheTagAndTakesItOffEveryCard()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(this.BuildCardTagged(100, this.Ours, _Chores), this.BuildCardTagged(101, this.Ours, _Chores));

        await this.HandleAsync(150);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Tag>().Should().BeEmpty();
        _ = this.Stored<ActivityTag>().Should().BeEmpty(
            "the join has no cascade on this side, so the rows go first or the delete is rejected");
    }

    [Fact]
    public async Task HandleAsync_LeavesTheCardsThemselvesStanding()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(this.BuildCardTagged(100, this.Ours, _Chores));

        await this.HandleAsync(150);

        _ = this.Stored<Activity>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOtherTagsOnACardAlone()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");
        var _Outside = BuildTag(151, this.Ours, "Outside");

        _ = this.Database.Seed(this.BuildCardTagged(100, this.Ours, _Chores, _Outside));

        await this.HandleAsync(150);

        _ = this.Stored<ActivityTag>().Select(t => t.TagID).Should().Equal([151L]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheTagBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(BuildTag(150, this.Ours, "Chores"), BuildTag(950, this.Theirs, "Theirs"));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Tag>().Should().HaveCount(2);
    }

    #endregion Methods

}
