using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.Tests.Infrastructure.Mapping;
using Home.Application.UseCases.Users.CreateUser;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.WebApi.Presenters.Users.CreateUser;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Users.CreateUser;

/// <summary>
/// Adding a member. The password never reaches the entity directly: it goes through
/// <see cref="IPasswordService"/>, and the mapper is configured to ignore it so a plain one cannot
/// arrive by accident.
/// </summary>
public class CreateUserInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<User>> m_AuditLogic = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly CreateUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(string email, string firstName, string lastName, string password, string middleNames = "")
        => new CreateUserInteractor().HandleAsync(
            new CreateUserInputPort(email, firstName, lastName, middleNames, password),
            this.m_Presenter,
            this.Services()
                .With(this.m_AuditLogic.Object)
                .With(this.m_PasswordService.Object)
                .With(TestMapper.Create())
                .Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WritesTheMemberIntoTheSignedInHousehold()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("ada@ours.test", "Ada", "Member", "a-password", "Grace");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<User>().Single();

        _ = _Stored.Email.Should().Be("ada@ours.test");
        _ = _Stored.FirstName.Should().Be("Ada");
        _ = _Stored.MiddleNames.Should().Be("Grace");
        _ = _Stored.LastName.Should().Be("Member");
        _ = this.Stored<User>().Count(u => u.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_PutsThePasswordThroughThePasswordServiceRatherThanStoringIt()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("ada@ours.test", "Ada", "Member", "a-password");

        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), "a-password"), Times.Once);
        _ = this.Stored<User>().Single().Password.Should().NotBe(
            "a-password",
            "the mapper ignores the password so a plain one cannot arrive on the entity by accident");
    }

    [Fact]
    public async Task HandleAsync_RecordsThatTheMemberWasAdded()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("ada@ours.test", "Ada", "Member", "a-password");

        this.m_AuditLogic.Verify(a => a.AddAudit(It.IsAny<User>()), Times.Once);
    }

    #endregion Methods

}
