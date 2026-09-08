using FluentAssertions;
using Home.Application.Infrastructure.Activities;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.SetActivityCompletion;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.Activities.SetActivityCompletion;
using Moq;

namespace Home.Application.Tests.UseCases.Activities.SetActivityCompletion;

/// <summary>
/// Who ticked a chore off is recorded from the session (8 Sep 2026), so "who did what" means
/// something once every member is themselves on their own device.
/// </summary>
public class CompletionAttributionTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<Activity>> m_AuditLogic = new();
    private readonly SetActivityCompletionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(long activityID, bool isComplete)
    {
        var _Services = this.Services(out var _Context);

        return new SetActivityCompletionInteractor().HandleAsync(
            new SetActivityCompletionInputPort(activityID, isComplete),
            this.m_Presenter,
            _Services
                .With<IActivityLogic>(new ActivityLogic(_Context, _Services.Time))
                .With(this.m_AuditLogic.Object)
                .Build(),
            CancellationToken.None);
    }

    private Activity SeedChore(User? completedBy = null, DateTime? completedOn = null)
    {
        var _Chore = new Activity()
        {
            ActivityID = 150,
            CompletedByUser = completedBy,
            CompletedDateUTC = completedOn,
            Household = this.Ours,
            Title = "Bins"
        };

        _ = this.Database.Seed(_Chore, this.Member);

        return _Chore;
    }

    [Fact]
    public async Task HandleAsync_TickingOffRecordsWhoIsSignedIn()
    {
        _ = this.SeedChore();

        await this.HandleAsync(150, true);

        _ = this.Stored<Activity>().Count(a => a.CompletedByUser != null && a.CompletedByUser.UserID == this.Member.UserID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_UntickingForgetsWhoDidIt()
    {
        _ = this.SeedChore(this.Member, new DateTime(2026, 9, 1));

        await this.HandleAsync(150, false);

        _ = this.Stored<Activity>().Count(a => a.CompletedByUser == null && a.CompletedDateUTC == null).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_TickingAnAlreadyDoneCardKeepsWhoDidItFirst()
    {
        var _Bo = new User() { UserID = 102, FirstName = "Bo", Household = this.Ours, LastName = "Member" };
        _ = this.SeedChore(_Bo, new DateTime(2026, 9, 1));

        await this.HandleAsync(150, true);

        _ = this.Stored<Activity>().Count(a => a.CompletedByUser != null && a.CompletedByUser.UserID == _Bo.UserID).Should().Be(
            1,
            "a second tap on an already-done card is not a second completion");
    }

    #endregion Methods

}
