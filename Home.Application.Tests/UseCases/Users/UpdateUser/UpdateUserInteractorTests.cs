using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.Tests.Infrastructure.Mapping;
using Home.Application.UseCases.Users.UpdateUser;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Home.WebApi.Presenters.Users.UpdateUser;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Users.UpdateUser;

/// <summary>
/// Editing a member, including changing a password. The mapper does the fields and the password
/// service does the password, so a member who does not send one keeps the one they have.
/// </summary>
public class UpdateUserInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<User>> m_AuditLogic = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly UpdateUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(
        long userID,
        PropertyChangeTracker<string> email = default,
        PropertyChangeTracker<string> firstName = default,
        PropertyChangeTracker<string> lastName = default,
        PropertyChangeTracker<string> middleNames = default,
        PropertyChangeTracker<string> password = default)
        => new UpdateUserInteractor().HandleAsync(
            new UpdateUserInputPort(email, firstName, lastName, middleNames, password, userID),
            this.m_Presenter,
            this.Services()
                .With(this.m_AuditLogic.Object)
                .With(this.m_PasswordService.Object)
                .With(TestMapper.Create())
                .Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_CorrectsAMemberNameAndSavesIt()
    {
        _ = this.Database.Seed(this.Member);

        await this.HandleAsync(this.Member.UserID, firstName: new("Adalovelace"), lastName: new("Corrected"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        var _Stored = this.Stored<User>().Single();

        _ = _Stored.FirstName.Should().Be("Adalovelace");
        _ = _Stored.LastName.Should().Be("Corrected");
    }

    [Fact]
    public async Task HandleAsync_WhenNoPasswordIsSent_LeavesThePasswordAlone()
    {
        _ = this.Database.Seed(this.Member);

        await this.HandleAsync(this.Member.UserID, firstName: new("Ada"));

        this.m_PasswordService.Verify(
            p => p.SetPassword(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never,
            "a member correcting a typo in their name must not be asked for a password too");
    }

    [Fact]
    public async Task HandleAsync_WhenAPasswordIsSent_PutsItThroughThePasswordService()
    {
        _ = this.Database.Seed(this.Member);

        await this.HandleAsync(this.Member.UserID, password: new("a-new-password"));

        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), "a-new-password"), Times.Once);
        _ = this.Stored<User>().Single().Password.Should().NotBe("a-new-password");
    }

    [Fact]
    public async Task HandleAsync_WhenTheMemberIsInAnotherHousehold_ChangesNothing()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour);

        await this.HandleAsync(this.Neighbour.UserID, firstName: new("Renamed by us"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<User>().Single(u => u.UserID == this.Neighbour.UserID).FirstName.Should().Be("Bo");
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<User>()), Times.Never);
    }

    #endregion Methods

}
