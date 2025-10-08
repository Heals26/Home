using AutoMapper;
using Home.Application.Infrastructure.Values;
using Home.Application.UseCases.OAuth.CreatePasswordGrant;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.Domain.Entities;
using Home.WebApi.UseCases.OAuth;
using Home.WebApi.UseCases.OAuth.CreatePasswordGrant;
using Home.WebApi.UseCases.OAuth.CreateRefreshGrant;

namespace Home.WebApi.Infrastructure.AutoMapper;

public class OAuthProfile : Profile
{

    #region Constructors

    public OAuthProfile()
    {
        _ = this.CreateMap<OAuthApiRequest, CreatePasswordGrantInputPort>();
        _ = this.CreateMap<UserAuthentication, CreatePasswordGrantApiResponse>()
            .ForMember(d => d.ExpiresIn, o => o.MapFrom(s => (s.DateSetUTC.AddYears(1) - DateTime.UtcNow).Ticks))
            .ForMember(d => d.GrantType, o => o.MapFrom(s => OAuthValues.GrantTypePassword))
            .ForMember(d => d.Scope, o => o.MapFrom(s => string.Join(",", OAuthValues.WebAppScope.Name)))
            .ForMember(d => d.UserID, o => o.MapFrom(s => s.User.UserID));

        _ = this.CreateMap<OAuthApiRequest, CreateRefreshGrantInputPort>();
        _ = this.CreateMap<UserAuthentication, CreateRefreshGrantApiResponse>()
            .ForMember(d => d.ExpiresIn, o => o.MapFrom(s => (s.DateSetUTC.AddYears(1) - DateTime.UtcNow).Ticks))
            .ForMember(d => d.GrantType, o => o.MapFrom(s => OAuthValues.GrantTypeRefresh))
            .ForMember(d => d.Scope, o => o.MapFrom(s => string.Join(",", OAuthValues.WebAppScope.Name)))
            .ForMember(d => d.UserID, o => o.MapFrom(s => s.User.UserID));
    }

    #endregion Constructors

}
