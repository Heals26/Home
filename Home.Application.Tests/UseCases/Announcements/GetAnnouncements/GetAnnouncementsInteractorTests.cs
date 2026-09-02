using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Announcements.GetAnnouncements;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Announcements.GetAnnouncements;
using Home.WebApi.UseCases.Announcements.GetAnnouncements;

namespace Home.Application.Tests.UseCases.Announcements.GetAnnouncements;

/// <summary>
/// The notes pinned to the family board. Newest first, because the dashboard shows the top of the
/// list and an old note pushing a new one out of sight is the whole failure mode.
/// </summary>
public class GetAnnouncementsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetAnnouncementsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Announcement BuildAnnouncement(long announcementID, Household household, string content, DateTime createdOnUTC)
        => new()
        {
            AnnouncementID = announcementID,
            Content = content,
            CreatedOnUTC = createdOnUTC,
            Household = household
        };

    private Task HandleAsync()
        => new GetAnnouncementsInteractor().HandleAsync(
            new GetAnnouncementsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsOurNotesNewestFirstAndNobodyElses()
    {
        _ = this.Database.Seed(
            BuildAnnouncement(110, this.Ours, "Bin night", new DateTime(2026, 8, 10, 8, 0, 0)),
            BuildAnnouncement(111, this.Ours, "Grandma here Saturday", new DateTime(2026, 8, 12, 8, 0, 0)),
            BuildAnnouncement(112, this.Ours, "Water the plants", new DateTime(2026, 8, 11, 8, 0, 0)),
            BuildAnnouncement(910, this.Theirs, "Their note", new DateTime(2026, 8, 13, 8, 0, 0)));

        await this.HandleAsync();

        _ = Ok<GetAnnouncementsApiResponse>(this.m_Presenter).Announcements
            .Select(a => a.Content).Should().Equal(
                ["Grandma here Saturday", "Water the plants", "Bin night"],
                "the newest note is the one the dashboard leads with");
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsPinned_PresentsAnEmptyList()
    {
        _ = this.Database.Seed(BuildAnnouncement(910, this.Theirs, "Their note", new DateTime(2026, 8, 13, 8, 0, 0)));

        await this.HandleAsync();

        _ = Ok<GetAnnouncementsApiResponse>(this.m_Presenter).Announcements.Should().BeEmpty();
    }

    #endregion Methods

}
