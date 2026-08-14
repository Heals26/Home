using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Announcements.DeleteAnnouncement;

public record DeleteAnnouncementInputPort(long AnnouncementID) : IInputPort<IDeleteAnnouncementOutputPort>;
