using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.GetUser;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Users.GetUser;
using Home.WebApi.UseCases.Users.GetUser;
using System.Text.Json;

namespace Home.Application.Tests.UseCases.Users.GetUser;

/// <summary>
/// One member. Nothing reaches this slice yet, which is how it came to answer 200 with an empty
/// body for a member it could not see, and to hand back the <c>User</c> entity whole — stored
/// password and all — for one it could.
/// </summary>
public class GetUserInteractorTests : InteractorTest
{

    #region Constants

    private const string StoredPassword = "a-stored-password-hash";

    #endregion Constants

    #region Fields

    private readonly GetUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static User BuildMember(long userID, Household household, string firstName, string middleNames, string lastName)
        => new()
        {
            Email = $"{firstName}@example.test".ToLowerInvariant(),
            FirstName = firstName,
            Household = household,
            LastName = lastName,
            MiddleNames = middleNames,
            Password = StoredPassword,
            UserID = userID
        };

    private Task HandleAsync(long userID)
        => new GetUserInteractor().HandleAsync(
            new GetUserInputPort(userID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheMemberIsOurs_PresentsTheirDetails()
    {
        _ = this.Database.Seed(BuildMember(100, this.Ours, "Ada", "Grace", "Member"));

        await this.HandleAsync(100);

        var _Response = Ok<GetUserApiResponse>(this.m_Presenter);

        _ = _Response.UserID.Should().Be(100);
        _ = _Response.Email.Should().Be("ada@example.test");
        _ = _Response.FirstName.Should().Be("Ada");
        _ = _Response.MiddleNames.Should().Be("Grace");
        _ = _Response.LastName.Should().Be("Member");
        _ = _Response.FullName.Should().Be("Ada Grace Member");
    }

    [Fact]
    public async Task HandleAsync_NeverPutsAStoredPasswordOnTheWire()
    {
        _ = this.Database.Seed(BuildMember(100, this.Ours, "Ada", string.Empty, "Member"));

        await this.HandleAsync(100);

        _ = JsonSerializer.Serialize(Ok<GetUserApiResponse>(this.m_Presenter)).Should()
            .NotContain(StoredPassword, "returning the entity whole handed every member the others' password hashes");
    }

    [Fact]
    public async Task HandleAsync_WhenTheMemberBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildMember(100, this.Ours, "Ada", string.Empty, "Member"),
            BuildMember(900, this.Theirs, "Bo", string.Empty, "Neighbour"));

        await this.HandleAsync(900);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchMemberExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildMember(100, this.Ours, "Ada", string.Empty, "Member"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
