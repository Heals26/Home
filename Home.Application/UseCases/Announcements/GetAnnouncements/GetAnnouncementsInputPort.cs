using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Announcements.GetAnnouncements;

public record GetAnnouncementsInputPort() : IInputPort<IGetAnnouncementsOutputPort>;
