using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.GetUsers;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Users.GetUsers;
using Home.WebApi.UseCases.Users.GetUsers;
using System.Text.Json;

namespace Home.Application.Tests.UseCases.Users.GetUsers;

/// <summary>
/// The household's members, as the Settings card lists them. Isolation matters more here than
/// anywhere: a leak would put another family's names and email addresses on the screen.
/// </summary>
public class GetUsersInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetUsersPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static User BuildMember(long userID, Household household, string firstName, string lastName)
        => new()
        {
            Email = $"{firstName}@example.test".ToLowerInvariant(),
            FirstName = firstName,
            Household = household,
            LastName = lastName,
            Password = "a-stored-password-hash",
            UserID = userID
        };

    private Task HandleAsync()
        => new GetUsersInteractor().HandleAsync(
            new GetUsersInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOurMembersByNameAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildMember(102, this.Ours, "Cy", "Younger"),
            BuildMember(101, this.Ours, "Cy", "Elder"),
            BuildMember(100, this.Ours, "Ada", "Member"),
            BuildMember(900, this.Theirs, "Bo", "Neighbour"));

        await this.HandleAsync();

        _ = Ok<GetUsersApiResponse>(this.m_Presenter).Users
            .Select(u => u.FullName).Should().Equal(
                ["Ada Member", "Cy Elder", "Cy Younger"],
                "members read by first name, with the surname breaking a tie");
    }

    [Fact]
    public async Task HandleAsync_NeverPutsAStoredPasswordOnTheWire()
    {
        _ = this.Database.Seed(BuildMember(100, this.Ours, "Ada", "Member"));

        await this.HandleAsync();

        _ = JsonSerializer.Serialize(Ok<GetUsersApiResponse>(this.m_Presenter)).Should()
            .NotContain("a-stored-password-hash", "a member list is not a place to hand out password hashes");
    }

    #endregion Methods

}
