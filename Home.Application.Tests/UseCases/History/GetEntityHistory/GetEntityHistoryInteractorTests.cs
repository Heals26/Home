using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.History.GetEntityHistory;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.History.GetEntityHistory;
using Home.WebApi.UseCases.History.GetEntityHistory;

namespace Home.Application.Tests.UseCases.History.GetEntityHistory;

/// <summary>
/// One thing's own history, behind the card on a chore and a recipe. The ID comes off the URL, so
/// the household link on the row is the only thing standing between a guessed number and another
/// family's records.
/// </summary>
public class GetEntityHistoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetEntityHistoryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Audit Row(long auditID, Household household, ResourceTypeSE entity, long entityID, string summary, User? user = null)
        => new()
        {
            AuditID = auditID,
            Content = "Add: whatever",
            Entity = entity,
            EntityID = entityID,
            Household = household,
            ModifiedDateUTC = new DateTime(2026, 9, 1).AddMinutes(auditID),
            Summary = summary,
            User = user,
            UserName = user?.UserName ?? "Bo Gone"
        };

    private Task HandleAsync(long entityID, ResourceTypeSE resourceType)
        => new GetEntityHistoryInteractor().HandleAsync(
            new GetEntityHistoryInputPort(entityID, resourceType.Value),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private GetEntityHistoryApiResponse Response()
        => Ok<GetEntityHistoryApiResponse>(this.m_Presenter);

    [Fact]
    public async Task HandleAsync_ReadsOneThingsHistoryNewestFirst()
    {
        _ = this.Database.Seed(
            Row(150, this.Ours, ResourceTypeSE.Activity, 500, "added the chore 'Bins'", this.Member),
            Row(151, this.Ours, ResourceTypeSE.Activity, 500, "ticked 'Bins' off", this.Member));

        await this.HandleAsync(500, ResourceTypeSE.Activity);

        _ = this.Response().Entries.Select(e => e.Summary).Should().Equal(
            ["ticked 'Bins' off", "added the chore 'Bins'"]);
    }

    [Fact]
    public async Task HandleAsync_LeavesOutAnotherThingOfTheSameKind()
    {
        _ = this.Database.Seed(
            Row(152, this.Ours, ResourceTypeSE.Activity, 500, "added the chore 'Bins'", this.Member),
            Row(153, this.Ours, ResourceTypeSE.Activity, 501, "added the chore 'Washing'", this.Member));

        await this.HandleAsync(500, ResourceTypeSE.Activity);

        _ = this.Response().Entries.Select(e => e.Summary).Should().Equal(["added the chore 'Bins'"]);
    }

    [Fact]
    public async Task HandleAsync_LeavesOutAnotherKindThatSharesTheID()
    {
        _ = this.Database.Seed(
            Row(154, this.Ours, ResourceTypeSE.Activity, 500, "added the chore 'Bins'", this.Member),
            Row(155, this.Ours, ResourceTypeSE.Recipe, 500, "added the recipe 'Bolognese'", this.Member));

        await this.HandleAsync(500, ResourceTypeSE.Recipe);

        _ = this.Response().Entries.Select(e => e.Summary).Should().Equal(["added the recipe 'Bolognese'"]);
    }

    [Fact]
    public async Task HandleAsync_NeverReadsAnotherHouseholdsHistoryForAGuessedID()
    {
        _ = this.Database.Seed(Row(950, this.Theirs, ResourceTypeSE.Activity, 500, "added the chore 'Their bins'", this.Neighbour));

        await this.HandleAsync(500, ResourceTypeSE.Activity);

        _ = this.Response().Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNothingForAKindThatDoesNotExist()
    {
        _ = this.Database.Seed(Row(156, this.Ours, ResourceTypeSE.Activity, 500, "added the chore 'Bins'", this.Member));

        await new GetEntityHistoryInteractor().HandleAsync(
            new GetEntityHistoryInputPort(500, -1),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

        _ = this.Response().Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_KeepsARowWrittenByAMemberWhoHasSinceGone()
    {
        _ = this.Database.Seed(Row(157, this.Ours, ResourceTypeSE.Activity, 500, "added the chore 'Bins'"));

        await this.HandleAsync(500, ResourceTypeSE.Activity);

        _ = this.Response().Entries.Single().Who.Should().Be("Bo Gone", "the name is copied onto the row so history survives a removal");
    }

    #endregion Methods

}
