using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Tags.GetTags;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Tags.GetTags;
using Home.WebApi.UseCases.Tags.GetTags;

namespace Home.Application.Tests.UseCases.Tags.GetTags;

/// <summary>
/// The household's labels. The colour rides along with the name because the board paints with it,
/// and a tag arriving without one renders as an unstyled chip.
/// </summary>
public class GetTagsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetTagsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Tag BuildTag(long tagID, Household household, string name, string colour)
        => new()
        {
            Colour = colour,
            Household = household,
            Name = name,
            TagID = tagID
        };

    private Task HandleAsync()
        => new GetTagsInteractor().HandleAsync(
            new GetTagsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOurTagsAlphabeticallyAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildTag(111, this.Ours, "Outside", "#00FF00"),
            BuildTag(110, this.Ours, "Chores", "#FF0000"),
            BuildTag(910, this.Theirs, "Admin", "#0000FF"));

        await this.HandleAsync();

        var _Tags = Ok<GetTagsApiResponse>(this.m_Presenter).Tags;

        _ = _Tags.Select(t => t.Name).Should().Equal(["Chores", "Outside"]);
        _ = _Tags.Select(t => t.Colour).Should().Equal(
            ["#FF0000", "#00FF00"],
            "the board paints the chip with this, so it has to travel with the name");
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdHasNoTags_PresentsAnEmptyList()
    {
        _ = this.Database.Seed(BuildTag(910, this.Theirs, "Admin", "#0000FF"));

        await this.HandleAsync();

        _ = Ok<GetTagsApiResponse>(this.m_Presenter).Tags.Should().BeEmpty();
    }

    #endregion Methods

}
