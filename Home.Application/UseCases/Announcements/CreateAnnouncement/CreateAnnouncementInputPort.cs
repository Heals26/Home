using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Announcements.CreateAnnouncement;

public record CreateAnnouncementInputPort(string Content) : IInputPort<ICreateAnnouncementOutputPort>;
