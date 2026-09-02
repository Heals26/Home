using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Activities.GetAssignedActivities;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Activities.GetAssignedActivities;
using Home.WebApi.UseCases.Activities.GetActivities;

namespace Home.Application.Tests.UseCases.Activities.GetAssignedActivities;

/// <summary>
/// "What's mine" — the one slice scoped to the signed-in member rather than the household. No
/// screen reaches it yet (roadmap B1), so these tests are what keeps it honest until one does.
/// </summary>
public class GetAssignedActivitiesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetAssignedActivitiesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Activity BuildCard(long activityID, Household household, User? assignedTo)
        => new()
        {
            ActivityID = activityID,
            Household = household,
            Title = $"Card {activityID}",
            User = assignedTo
        };

    private Task HandleAsync()
        => new GetAssignedActivitiesInteractor().HandleAsync(
            new GetAssignedActivitiesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOnlyTheCardsAssignedToTheSignedInMember()
    {
        var _Housemate = new User() { UserID = 101, Email = "housemate@ours.test", FirstName = "Cy", Household = this.Ours, LastName = "Housemate" };

        _ = this.Database.Seed(
            this.BuildCard(110, this.Ours, this.Member),
            this.BuildCard(111, this.Ours, _Housemate),
            this.BuildCard(112, this.Ours, null),
            this.BuildCard(910, this.Theirs, this.Neighbour));

        await this.HandleAsync();

        _ = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities
            .Select(a => a.ActivityID).Should().Equal([110], "a card assigned to somebody else is not mine");
    }

    [Fact]
    public async Task HandleAsync_NamesTheMemberOnTheirOwnCards()
    {
        _ = this.Database.Seed(this.BuildCard(110, this.Ours, this.Member));

        await this.HandleAsync();

        var _Presented = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities.Single();

        _ = _Presented.AssignedToUserID.Should().Be(this.Member.UserID);
        _ = _Presented.AssignedTo.Should().Be(this.Member.UserName);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsAssignedToTheMember_PresentsAnEmptyList()
    {
        _ = this.Database.Seed(this.BuildCard(112, this.Ours, null));

        await this.HandleAsync();

        _ = Ok<GetActivitiesApiResponse>(this.m_Presenter).Activities.Should().BeEmpty();
    }

    #endregion Methods

}
