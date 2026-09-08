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
/// A member does not have to be able to sign in (8 Sep 2026). A child is someone to assign things
/// to and name on events; the login comes later, if ever.
/// </summary>
public class CreateMemberWithoutLoginTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<User>> m_AuditLogic = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly CreateUserPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(string? email, string? password)
        => new CreateUserInteractor().HandleAsync(
            new CreateUserInputPort(email, "Ava", "Member", string.Empty, password),
            this.m_Presenter,
            this.Services()
                .With(this.m_AuditLogic.Object)
                .With(this.m_PasswordService.Object)
                .With(TestMapper.Create())
                .Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WithNeitherEmailNorPassword_StoresAMemberWhoCannotSignIn()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(null, null);

        var _Stored = this.Stored<User>().Single();

        _ = _Stored.Email.Should().BeNull();
        _ = _Stored.Password.Should().BeNull();
        _ = _Stored.HasLogin.Should().BeFalse();
        _ = _Stored.FirstName.Should().Be("Ava");
        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithBlankEmailAndPassword_TreatsThemAsAbsent()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("   ", string.Empty);

        _ = this.Stored<User>().Single().Email.Should().BeNull("whitespace is not an address anyone can sign in with");
    }

    [Fact]
    public async Task HandleAsync_WithBoth_TrimsTheEmailAndSetsThePassword()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("  ava@ours.test ", "a-password");

        _ = this.Stored<User>().Single().Email.Should().Be("ava@ours.test");
        this.m_PasswordService.Verify(p => p.SetPassword(It.IsAny<User>(), "a-password"), Times.Once);
    }

    #endregion Methods

}
