using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.DeleteShoppingList;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.DeleteShoppingList;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.DeleteShoppingList;

/// <summary>
/// Throwing a shopping list away, which is the destructive one. Archiving is the reversible
/// alternative and lives on the update slice.
/// </summary>
public class DeleteShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly DeleteShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household)
        => new()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

    private Task HandleAsync(long shoppingListID)
        => new DeleteShoppingListInteractor().HandleAsync(
            new DeleteShoppingListInputPort(shoppingListID),
            this.m_Presenter,
            this.Services().With(this.m_AuditLogic.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOurList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours), BuildList(121, this.Ours));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingList>().Select(sl => sl.ShoppingListID).Should().Equal([121]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_KeepsItAndStillAnswersNoContent()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours), BuildList(920, this.Theirs));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingList>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_RecordsOnlyTheListItActuallyDeleted()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs));

        await this.HandleAsync(920);

        this.m_AuditLogic.Verify(a => a.DeleteAudit(It.IsAny<ShoppingList>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchListExists_AnswersNoContentSoDeletingTwiceIsHarmless()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(404);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingList>().Should().ContainSingle();
    }

    #endregion Methods

}
