using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.SetActivityTags;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Activities.SetActivityTags;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Activities.SetActivityTags;

/// <summary>
/// Replacing the whole set of labels on a card. The caller sends what the card should end up with
/// rather than what to add or remove, so two tablets editing the same card cannot end up with
/// different answers.
/// </summary>
public class SetActivityTagsInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetActivityTagsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Tag BuildTag(long tagID, Household household, string name)
        => new()
        {
            Colour = "#FF0000",
            Household = household,
            Name = name,
            TagID = tagID
        };

    private Activity BuildCard(long activityID, Household household, params Tag[] tags)
    {
        var _Activity = new Activity() { ActivityID = activityID, Household = household, Title = $"Card {activityID}" };

        _Activity.Tags = [.. tags.Select(t => new ActivityTag() { Activity = _Activity, Tag = t })];

        return _Activity;
    }

    private Task HandleAsync(long activityID, params long[] tagIDs)
        => new SetActivityTagsInteractor().HandleAsync(
            new SetActivityTagsInputPort(activityID, tagIDs),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AddsTheTagsTheCardDidNotHave()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");
        var _Outside = BuildTag(151, this.Ours, "Outside");

        _ = this.Database.Seed(_Outside, this.BuildCard(100, this.Ours, _Chores));

        await this.HandleAsync(100, 150, 151);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ActivityTag>().Select(t => t.TagID).Should().BeEquivalentTo([150L, 151L]);
    }

    [Fact]
    public async Task HandleAsync_RemovesTheTagsLeftOutOfTheSet()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");
        var _Outside = BuildTag(151, this.Ours, "Outside");

        _ = this.Database.Seed(this.BuildCard(100, this.Ours, _Chores, _Outside));

        await this.HandleAsync(100, 150);

        _ = this.Stored<ActivityTag>().Select(t => t.TagID).Should().Equal([150L]);
    }

    [Fact]
    public async Task HandleAsync_WithAnEmptySetTakesEveryTagOff()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(this.BuildCard(100, this.Ours, _Chores));

        await this.HandleAsync(100);

        _ = this.Stored<ActivityTag>().Should().BeEmpty();
        _ = this.Stored<Tag>().Should().ContainSingle("taking a label off a card does not delete the label");
    }

    [Fact]
    public async Task HandleAsync_IgnoresARepeatedTagRatherThanAddingItTwice()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(_Chores, this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, 150, 150);

        _ = this.Stored<ActivityTag>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenATagBelongsToAnotherHousehold_RefusesTheWholeSet()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");
        var _Theirs = BuildTag(950, this.Theirs, "Theirs");

        _ = this.Database.Seed(_Chores, _Theirs, this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, 150, 950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityTag>().Should().BeEmpty(
            "one bad ID refuses the lot rather than quietly applying the half it recognised");
    }

    [Fact]
    public async Task HandleAsync_WhenATagDoesNotExist_RefusesTheWholeSet()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(_Chores, this.BuildCard(100, this.Ours));

        await this.HandleAsync(100, 150, 404);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityTag>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheCardBelongsToAnotherHousehold_PresentsNotFound()
    {
        var _Chores = BuildTag(150, this.Ours, "Chores");

        _ = this.Database.Seed(_Chores, this.BuildCard(900, this.Theirs));

        await this.HandleAsync(900, 150);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ActivityTag>().Should().BeEmpty();
    }

    #endregion Methods

}
