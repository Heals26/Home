using AutoMapper;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserProfile : Profile
{

    #region Constructors

    public CreateUserProfile()
        => this.CreateMap<CreateUserInputPort, User>()
            .ForMember(d => d.UserID, o => o.Ignore())
            .ForMember(d => d.AssignedActivities, o => o.Ignore())
            .ForMember(d => d.Audits, o => o.Ignore())
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.PasswordLastChanged, o => o.Ignore())
            .ForMember(d => d.UserName, o => o.Ignore());

    #endregion Constructors

}
