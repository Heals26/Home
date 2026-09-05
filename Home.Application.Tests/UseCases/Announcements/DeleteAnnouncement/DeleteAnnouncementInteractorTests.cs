using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Announcements.DeleteAnnouncement;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Announcements.DeleteAnnouncement;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Announcements.DeleteAnnouncement;

/// <summary>
/// Taking a note off the family board.
/// </summary>
public class DeleteAnnouncementInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteAnnouncementPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Announcement BuildAnnouncement(long announcementID, Household household, string content)
        => new()
        {
            AnnouncementID = announcementID,
            Content = content,
            CreatedOnUTC = new DateTime(2026, 8, 12),
            Household = household
        };

    private Task HandleAsync(long announcementID)
        => new DeleteAnnouncementInteractor().HandleAsync(
            new DeleteAnnouncementInputPort(announcementID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOnlyThatNote()
    {
        _ = this.Database.Seed(
            BuildAnnouncement(110, this.Ours, "Bin night"),
            BuildAnnouncement(111, this.Ours, "Grandma Saturday"));

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Announcement>().Select(a => a.Content).Should().Equal(["Grandma Saturday"]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildAnnouncement(110, this.Ours, "Bin night"),
            BuildAnnouncement(910, this.Theirs, "Theirs"));

        await this.HandleAsync(910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Announcement>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchNoteExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildAnnouncement(110, this.Ours, "Bin night"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
