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
/// A member who started without a login can be given one later, and one can be taken away by
/// blanking the email. A password on its own is refused, because nothing can sign in without an
/// address to sign in with.
/// </summary>
public class GiveMemberALoginTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<User>> m_AuditLogic = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly UpdateUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private User SeedChild()
    {
        var _Child = new User() { UserID = 101, FirstName = "Ava", Household = this.Ours, LastName = "Member" };

        _ = this.Database.Seed(_Child);

        return _Child;
    }

    private Task HandleAsync(long userID, PropertyChangeTracker<string> email, PropertyChangeTracker<string> password)
        => new UpdateUserInteractor().HandleAsync(
            new UpdateUserInputPort(email, default, default, default, password, userID),
            this.m_Presenter,
            this.Services()
                .With(this.m_AuditLogic.Object)
                .With(this.m_PasswordService.Object)
                .With(TestMapper.Create())
                .Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AnEmailAndPasswordTogetherGiveTheMemberALogin()
    {
        var _Child = this.SeedChild();

        await this.HandleAsync(_Child.UserID, new("ava@ours.test"), new("a-password"));

        _ = this.Stored<User>().Single().Email.Should().Be("ava@ours.test");
        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), "a-password"), Times.Once);
        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_APasswordWithoutAnEmailIsRefused()
    {
        var _Child = this.SeedChild();

        await this.HandleAsync(_Child.UserID, default, new("a-password"));

        _ = this.m_Presenter.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BlankingTheEmailTakesTheLoginAway()
    {
        var _Adult = new User() { UserID = 102, Email = "bo@ours.test", FirstName = "Bo", Household = this.Ours, LastName = "Member", Password = "hashed", PasswordLastChanged = new DateTime(2026, 1, 1) };
        _ = this.Database.Seed(_Adult);

        await this.HandleAsync(_Adult.UserID, new(string.Empty), default);

        var _Stored = this.Stored<User>().Single();

        _ = _Stored.Email.Should().BeNull();
        _ = _Stored.Password.Should().BeNull();
        _ = _Stored.HasLogin.Should().BeFalse();
    }

    #endregion Methods

}
