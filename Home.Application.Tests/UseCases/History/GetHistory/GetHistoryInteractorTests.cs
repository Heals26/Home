using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.History.GetHistory;
using Home.Application.UseCases.History.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.History.GetHistory;
using Home.WebApi.UseCases.History.GetHistory;

namespace Home.Application.Tests.UseCases.History.GetHistory;

/// <summary>
/// The household's feed: newest first, one household only, narrowed by kind, paged by the last row
/// shown. Rows are scoped by their own household link, so a removed member's doings stay.
/// </summary>
public class GetHistoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetHistoryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Audit Row(long auditID, Household household, ResourceTypeSE entity, string summary, User? user = null)
        => new()
        {
            AuditID = auditID,
            Content = "Add: whatever",
            Entity = entity,
            EntityID = auditID,
            Household = household,
            ModifiedDateUTC = new DateTime(2026, 9, 1).AddMinutes(auditID),
            Summary = summary,
            User = user,
            UserName = user?.UserName ?? "Bo Gone"
        };

    private Task HandleAsync(int take = 25, long? beforeAuditID = null, params HistoryCategory[] categories)
        => new GetHistoryInteractor().HandleAsync(
            new GetHistoryInputPort(beforeAuditID, categories, take),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private GetHistoryApiResponse Response()
        => Ok<GetHistoryApiResponse>(this.m_Presenter);

    [Fact]
    public async Task HandleAsync_ReadsNewestFirstWithWhoAndWords()
    {
        _ = this.Database.Seed(
            Row(150, this.Ours, ResourceTypeSE.Activity, "added the chore 'Bins'", this.Member),
            Row(151, this.Ours, ResourceTypeSE.MealPlanEntry, "planned Tacos for Wednesday dinner", this.Member));

        await this.HandleAsync();

        var _Entries = this.Response().Entries;

        _ = _Entries.Select(e => e.AuditID).Should().Equal(151, 150);
        _ = _Entries[0].Who.Should().Be("Ada Member");
        _ = _Entries[0].Summary.Should().Be("planned Tacos for Wednesday dinner");
        _ = _Entries[0].Category.Should().Be(HistoryCategory.Meals);
        _ = _Entries[1].Category.Should().Be(HistoryCategory.Chores);
    }

    [Fact]
    public async Task HandleAsync_NeverShowsAnotherHouseholdsHistory_ButKeepsARemovedMembersDoings()
    {
        _ = this.Database.Seed(
            Row(150, this.Ours, ResourceTypeSE.Activity, "ticked off 'Bins'"),
            Row(950, this.Theirs, ResourceTypeSE.Activity, "ticked off 'Their bins'", this.Neighbour));

        await this.HandleAsync();

        var _Entries = this.Response().Entries;

        _ = _Entries.Select(e => e.Summary).Should().Equal(["ticked off 'Bins'"]);
        _ = _Entries[0].Who.Should().Be("Bo Gone", "the name is kept on the row after the member is gone");
    }

    [Fact]
    public async Task HandleAsync_NarrowsByCategory()
    {
        _ = this.Database.Seed(
            Row(150, this.Ours, ResourceTypeSE.Activity, "ticked off 'Bins'"),
            Row(151, this.Ours, ResourceTypeSE.ShoppingCart, "started the list 'Weekly'"),
            Row(152, this.Ours, ResourceTypeSE.CalendarEvent, "put 'Swimming' on the calendar"),
            Row(153, this.Ours, ResourceTypeSE.CalendarSubscription, "subscribed to the calendar 'School'"));

        await this.HandleAsync(25, null, HistoryCategory.Calendar, HistoryCategory.Shopping);

        _ = this.Response().Entries.Select(e => e.AuditID).Should().Equal(
            [153, 152, 151],
            "both calendar resource types fall under Calendar");
    }

    [Fact]
    public async Task HandleAsync_PagesFromTheLastRowShown()
    {
        _ = this.Database.Seed(
            Row(150, this.Ours, ResourceTypeSE.Activity, "one"),
            Row(151, this.Ours, ResourceTypeSE.Activity, "two"),
            Row(152, this.Ours, ResourceTypeSE.Activity, "three"));

        await this.HandleAsync(take: 2);

        var _First = this.Response();

        _ = _First.Entries.Select(e => e.AuditID).Should().Equal(152, 151);
        _ = _First.HasMore.Should().BeTrue();

        await this.HandleAsync(take: 2, beforeAuditID: 151);

        var _Second = this.Response();

        _ = _Second.Entries.Select(e => e.AuditID).Should().Equal(150);
        _ = _Second.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_GivesARowWrittenBeforeSummariesABlandOne()
    {
        var _Old = Row(150, this.Ours, ResourceTypeSE.Recipe, null!);
        _Old.Summary = null;

        _ = this.Database.Seed(_Old);

        await this.HandleAsync();

        _ = this.Response().Entries.Single().Summary.Should().Be("added a recipe");
    }

    #endregion Methods

}
