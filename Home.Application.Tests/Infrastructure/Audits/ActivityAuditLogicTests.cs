using FluentAssertions;
using Home.Application.Logic.Audits;
using Home.Application.Tests.Infrastructure;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.Tests.Infrastructure.Audits;

/// <summary>
/// The words the feed will show for a chore. A row is written against the real change tracker, so
/// "ticked off" against "renamed" is decided by what actually changed, not by what was passed in.
/// </summary>
public class ActivityAuditLogicTests : InteractorTest
{

    #region Methods

    private (IAuditLogic<Activity> Logic, Activity Chore, Home.Application.Services.Persistence.IPersistenceContext Context) Arrange(ActivityState? state = null)
    {
        var _Chore = new Activity() { ActivityID = 150, Household = this.Ours, State = state, Title = "Bins" };

        _ = this.Database.Seed(_Chore, this.Member);

        var _Services = this.Services(out var _Context);
        var _Loaded = _Context.GetEntities<Activity>().Single(a => a.ActivityID == 150);

        return (new ActivityAuditLogic(this.AuthorisationService.Object, _Context, _Services.Time), _Loaded, _Context);
    }

    [Fact]
    public async Task UpdateAudit_TickingOffSaysSo_AndStampsWhoAndWhichHousehold()
    {
        var (_Logic, _Chore, _Context) = this.Arrange();

        _Chore.CompletedDateUTC = TestServiceFactory.DefaultNow.UtcDateTime;
        _Logic.UpdateAudit(_Chore);
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        var _Audit = this.Stored<Audit>().Single();

        _ = _Audit.Summary.Should().Be("ticked off 'Bins'");
        _ = _Audit.UserName.Should().Be("Ada Member");
        _ = this.Stored<Audit>().Count(a => a.Household != null && a.Household.HouseholdID == OurHouseholdID).Should().Be(1);
        _ = _Audit.ModifiedDateUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task UpdateAudit_RenamingNamesBothEnds()
    {
        var (_Logic, _Chore, _Context) = this.Arrange();

        _Chore.Title = "Bins and recycling";
        _Logic.UpdateAudit(_Chore);
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        _ = this.Stored<Audit>().Single().Summary.Should().Be("renamed 'Bins' to 'Bins and recycling'");
    }

    [Fact]
    public async Task DeleteAudit_WritesARemovalRatherThanForgetting()
    {
        var (_Logic, _Chore, _Context) = this.Arrange();

        _Logic.UpdateAudit(_Chore, "changed 'Bins'");
        _Logic.DeleteAudit(_Chore);
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        _ = this.Stored<Audit>().OrderBy(a => a.AuditID).Select(a => a.Summary).Should().Equal(
            ["changed 'Bins'", "removed the chore 'Bins'"],
            "history outlives the thing it describes");
    }

    [Fact]
    public async Task AddAudit_OnANewChore_EndsUpWithTheIDTheDatabaseAssigned()
    {
        _ = this.Database.Seed(this.Member);

        var _Services = this.Services(out var _Context);
        IAuditLogic<Activity> _Logic = new ActivityAuditLogic(this.AuthorisationService.Object, _Context, _Services.Time);
        var _Chore = new Activity() { Household = _Context.GetEntities<Household>().Single(h => h.HouseholdID == OurHouseholdID), Title = "New bins" };

        _Context.Add(_Chore);
        _Logic.AddAudit(_Chore);
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        _ = _Chore.ActivityID.Should().NotBe(0);
        _ = this.Stored<Audit>().Single().EntityID.Should().Be(_Chore.ActivityID, "the row was written before the insert gave the chore an ID");
    }

    [Fact]
    public async Task AddAudit_WithACallerSummary_KeepsTheCallersWords()
    {
        var (_Logic, _Chore, _Context) = this.Arrange();

        _Logic.AddAudit(_Chore, "brought 'Bins' back");
        _ = await _Context.SaveChangesAsync(CancellationToken.None);

        _ = this.Stored<Audit>().Single().Summary.Should().Be("brought 'Bins' back");
    }

    #endregion Methods

}
