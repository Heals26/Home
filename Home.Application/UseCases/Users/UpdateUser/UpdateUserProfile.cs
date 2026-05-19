using AutoMapper;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.UpdateUser;

public class UpdateUserProfile : Profile
{

    #region Constructors

    public UpdateUserProfile()
    {
        this.CreateMap<UpdateUserInputPort, User>()
            .ForMember(d => d.UserID, o => o.Ignore())
            .ForMember(d => d.AssignedActivities, o => o.Ignore())
            .ForMember(d => d.Audits, o => o.Ignore())
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.PasswordLastChanged, o => o.Ignore())
            .ForMember(d => d.UserName, o => o.Ignore())
            .ForMember(d => d.Email, o =>
            {
                o.Condition(s => s.Email.HasBeenSet);
                o.MapFrom(s => s.Email.Value);
            })
            .ForMember(d => d.FirstName, o =>
            {
                o.Condition(s => s.FirstName.HasBeenSet);
                o.MapFrom(s => s.FirstName.Value);
            })
            .ForMember(d => d.LastName, o =>
            {
                o.Condition(s => s.LastName.HasBeenSet);
                o.MapFrom(s => s.LastName.Value);
            })
            .ForMember(d => d.MiddleNames, o =>
            {
                o.Condition(s => s.MiddleNames.HasBeenSet);
                o.MapFrom(s => s.MiddleNames.Value);
            });
    }

    #endregion Constructors

}
