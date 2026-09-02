using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CardSections.GetCardSections;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CardSections.GetCardSections;
using Home.WebApi.UseCases.CardSections.GetCardSections;

namespace Home.Application.Tests.UseCases.CardSections.GetCardSections;

/// <summary>
/// The household's card headings, and the count that decides whether one can still be deleted.
/// <para>
/// That count is the reason the regions are projected. Left unprojected the collection arrives
/// empty, every section reports zero cards, and the settings sheet cheerfully offers to delete one
/// with a card's writing under it — which is exactly how it shipped on 1 Sep before it was caught
/// by hand.
/// </para>
/// </summary>
public class GetCardSectionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetCardSectionsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync()
        => new GetCardSectionsInteractor().HandleAsync(
            new GetCardSectionsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    /// <summary>
    /// Our household with Details used by two cards, Steps used by none, and a section belonging
    /// to the neighbours.
    /// </summary>
    private object[] BuildSections()
    {
        var _Details = new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "Details", Sequence = 1 };
        var _Steps = new CardSection() { CardSectionID = 111, Household = this.Ours, Name = "Steps", Sequence = 2 };
        var _Theirs = new CardSection() { CardSectionID = 910, Household = this.Theirs, Name = "Acceptance Criteria", Sequence = 1 };

        var _FirstCard = new Activity() { ActivityID = 100, Household = this.Ours, Title = "Clean the balcony" };
        _FirstCard.Regions = [new ActivityRegion() { ActivityRegionID = 130, Activity = _FirstCard, CardSection = _Details, Sequence = 1 }];

        var _SecondCard = new Activity() { ActivityID = 101, Household = this.Ours, Title = "Mow the lawn" };
        _SecondCard.Regions = [new ActivityRegion() { ActivityRegionID = 131, Activity = _SecondCard, CardSection = _Details, Sequence = 1 }];

        return [_Steps, _Theirs, _FirstCard, _SecondCard];
    }

    [Fact]
    public async Task HandleAsync_CountsTheCardsUsingEachSection()
    {
        _ = this.Database.Seed(this.BuildSections());

        await this.HandleAsync();

        var _Sections = Ok<GetCardSectionsApiResponse>(this.m_Presenter).CardSections;

        _ = _Sections.Single(s => s.Name == "Details").CardCount.Should().Be(
            2,
            "an unprojected region collection counts zero and offers to delete a section in use");
        _ = _Sections.Single(s => s.Name == "Steps").CardCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOurSectionsInReadingOrderAndNobodyElses()
    {
        _ = this.Database.Seed(this.BuildSections());

        await this.HandleAsync();

        _ = Ok<GetCardSectionsApiResponse>(this.m_Presenter).CardSections
            .Select(s => s.Name).Should().Equal(
                ["Details", "Steps"],
                "another household's headings are not ours, whatever they call them");
    }

    [Fact]
    public async Task HandleAsync_WhenTwoSectionsShareASequence_BreaksTheTieOnID()
    {
        _ = this.Database.Seed(
            new CardSection() { CardSectionID = 111, Household = this.Ours, Name = "Second", Sequence = 1 },
            new CardSection() { CardSectionID = 110, Household = this.Ours, Name = "First", Sequence = 1 });

        await this.HandleAsync();

        _ = Ok<GetCardSectionsApiResponse>(this.m_Presenter).CardSections
            .Select(s => s.CardSectionID).Should().Equal(110, 111);
    }

    #endregion Methods

}
